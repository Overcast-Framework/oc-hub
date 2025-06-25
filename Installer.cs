using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace oc_hub
{
    public class Installer
    {
        public class ReleaseInfo
        {
            public string tag_name { get; set; }
            public string html_url { get; set; }
            public Asset[] assets { get; set; }
        }

        public class Asset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }

        public async Task<ReleaseInfo> GetLatestReleaseAsync()
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OvercastInstaller/1.0");

            try
            {
                string url = "https://api.github.com/repos/Overcast-Framework/OvercastC/releases/latest";
                var response = await client.GetStringAsync(url);

                var release = JsonConvert.DeserializeObject<ReleaseInfo>(response);

                return release;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            throw new Exception("idk how we got here");
        }

        public async void Install()
        {
            if(Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Console.WriteLine("oc-hub does not support non-Windows machines yet");
                return;
            }

            // first: create all the directories
            // mainly stuff resides in C:/Program Files/Overcast

            Console.WriteLine("Creating Directories...");

            try
            {
                var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Overcast");
                Directory.CreateDirectory(basePath);
                Directory.CreateDirectory(Path.Combine(basePath, "libraries")); // unused currently, but just future-proofing

                // then just install the binary
                Console.WriteLine("Downloading latest release...");

                // query the latest release from GH
                var releaseData = await GetLatestReleaseAsync();
                Console.WriteLine("Latest release: " + releaseData.tag_name);

                using WebClient wc = new WebClient();

                wc.DownloadProgressChanged += (s, e) =>
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(e.ProgressPercentage + "% [" + (e.BytesReceived)/1000000 + " MB]");
                };

                wc.DownloadFileAsync(new Uri(releaseData.assets.FirstOrDefault(e => e.name == "overcastc-bin-win64.zip").browser_download_url), Path.Combine(basePath, "overcast.zip"));

                while (wc.IsBusy)
                    Thread.Sleep(100);

                Console.WriteLine("\nDownloaded latest release");
                Console.WriteLine("Unarchiving release zip...");

                ZipFile.ExtractToDirectory(Path.Combine(basePath, "overcast.zip"), basePath);

                Console.WriteLine("Cleaning up...");
                File.Delete(Path.Combine(basePath, "overcast.zip"));

                Console.WriteLine("Adding to PATH and env variables");

                Environment.SetEnvironmentVariable("OvercastInstall", basePath, EnvironmentVariableTarget.User);
                string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
                if (!path.Contains(basePath))
                {
                    path += ";" + basePath;
                    Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.User);
                }

                // create a version tracker
                File.WriteAllText(Path.Combine(basePath, "VERSION"), releaseData.tag_name);
                File.Move(Path.Combine(basePath, "Overcast.exe"), Path.Combine(basePath, "overcast.exe")); // for niceness

                Console.WriteLine("Installation complete!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Installation Error: " + e.Message);
                return;
            }
        }

        public void Uninstall()
        {

        }
    }
}
