using FortRise.Installer;
using System.IO;
using System;
using System.Reflection;
using Internal;

internal class Program 
{
    public static string Version = "1.0.0";

    [STAThread]
    public static void Main(string[] args)
    {
        Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (args.Length == 0) 
        {
            Console.WriteLine("--patch <path/to/TowerFall>");
            Console.WriteLine("--unpatch <path/to/TowerFall>");
            return;
        }
        if (args.Length <= 1)
        {
            return;
        }
        if (!File.Exists(Path.Combine(args[1], "TowerFall.exe")))
        {
            Console.WriteLine("TowerFall.exe not found in this directory: " + args[1]);
            return;
        }
        try
        {
            Console.WriteLine("Creating Sandbox App Domain");
            AppDomain domain = null;
            var app = new AppDomainSetup();
            app.ApplicationBase = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            app.LoaderOptimization = LoaderOptimization.SingleDomain;

            domain = AppDomain.CreateDomain(
                AppDomain.CurrentDomain.FriendlyName + " FortRise Installer",
                AppDomain.CurrentDomain.Evidence,
                app,
                AppDomain.CurrentDomain.PermissionSet
            );

            Console.WriteLine("Created " + AppDomain.CurrentDomain.FriendlyName + " FortRise Installer");

            var installer = (Installer)domain.CreateInstanceAndUnwrap(
                typeof(Installer).Assembly.FullName,
                typeof(Installer).FullName
            );
            if (args[0] == "--patch")
            {
                Console.WriteLine("Installing FortRise");
                installer.Install(args[1]);
            }
            else if (args[0] == "--unpatch")
            {
                Console.WriteLine("Uninstalling FortRise");
                installer.Uninstall(args[1]);
                return;
            }

            Console.WriteLine("Unloading Sandbox App Domain");
            AppDomain.Unload(domain);
            Console.WriteLine("Unloaded");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("Installer failed!");
        }
    }
}