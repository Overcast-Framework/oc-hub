namespace oc_hub
{
    internal class Program
    {
        const string OC_HUB_VERSION = "v1.0.0";

        static void Main(string[] args)
        {
            string helpText = @"oc-hub install | installs the latest version of OvercastC
oc-hub uninstall | uninstalls OvercastC
oc-hub version | prints the version of OvercastC and oc-hub installed
oc-hub help | brings up this help page
            ";
            
            if(args.Length == 0)
            {
                Console.WriteLine(helpText);
            }
            else
            {
                switch(args[0])
                {
                    case "install":
                        new Installer().Install();
                        break;
                    case "uninstall":
                        new Installer().Uninstall();
                        break;
                    case "version":
                        Console.WriteLine("oc-hub: " + OC_HUB_VERSION);
                        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Overcast");
                        if (Directory.Exists(basePath))
                        {
                            var ocVersion = File.ReadAllText(Path.Combine(basePath, "VERSION"));
                            Console.WriteLine("overcast: " + ocVersion);
                        }
                        else
                        {
                            Console.WriteLine("No Overcast Installation Detected");
                        }
                        break;
                    case "help":
                        Console.WriteLine(helpText);
                        break;
                }
            }

            Console.ReadKey();
        }
    }
}
