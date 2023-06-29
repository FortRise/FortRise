using FortRise.Installer;
using System.Threading.Tasks;
using System.IO;
using System;
#if ANSI
using NativeFileDialogSharp;
using Spectre.Console;
#endif

internal class Program 
{
    public static string? Version = "1.0.0";
    public static bool DebugMode = false;
    public static bool FNA = true;

    [STAThread]
    public async static Task Main(string[] args) 
    {
        Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (args.Length > 1) 
        {
            if (!File.Exists(args[1] + "/TowerFall.exe")) 
            {
                Console.WriteLine("TowerFall executable not found");
                return;
            }
            if (args[0] == "--patch") 
            {
                
                await Installer.Install(args[1]);
                return;
            }
            else if (args[0] == "--unpatch") 
            {
                await Installer.Uninstall(args[1]);
                return;
            }
            if (args.Length > 2 && args[3] == "--debug") 
            {
                for (int i = 3; i < 6; i++) 
                {
                    var arg = args[i];
                    switch (arg) 
                    {
                    case "--debug":
                        DebugMode = true;
                        break;
                    case "--fna":
                        FNA = true;
                        break;
                    }
                }
            }
        }

#if ANSI
        Load();
        var panel = new Panel("FortRise Installer v" + Version) {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 2, 2, 2),
            Expand = true
        };

        AnsiConsole.Write(panel);

        while (true) 
        {
            var stateSelection = AnsiConsole.Prompt(
                new SelectionPrompt<MenuState>()
                    .Title("[underline]Main Menu[/]")
                    .PageSize(10)
                    .AddChoices(new [] { MenuState.Patch, MenuState.Unpatch, MenuState.Settings, MenuState.Quit})
            );

            switch (stateSelection) 
            {
            case MenuState.Patch:
                await StatePatch();
                break;
            case MenuState.Unpatch:
                await StateUnpatch();
                break;
            case MenuState.Settings:
                OpenSettings();
                break;
            case MenuState.Quit:
                goto End;
            }
        }
        End:
        AnsiConsole.WriteLine("Goodbye!");
#endif
    }
#if ANSI
    public static void OpenSettings() 
    {
        while (true) 
        {
            var settingsSelection = AnsiConsole.Prompt(
                new SelectionPrompt<SettingsState>()
                    .Title($"[underline]Settings[/]\nFNA: {(FNA ? "ON" : "OFF")}\nDebug: {(DebugMode ? "ON" : "OFF")}")
                    .PageSize(10)
                    .AddChoices(new [] { SettingsState.Debug, SettingsState.FNA, SettingsState.Quit })
            );

            switch (settingsSelection) 
            {
            case SettingsState.Debug:
                DebugMode = !DebugMode;
                Save();
                continue;
            case SettingsState.FNA:
                FNA = !FNA;
                Save();
                continue;
            case SettingsState.Quit:
                goto End;
            }
            End:
            break;
        }
    }

    public static void Save() 
    {
        var json = new TeuJson.JsonObject() 
        {
            ["FNA"] = FNA,
            ["DEBUG"] = DebugMode
        };

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var combined = Path.Combine(appData, "FortRiseInstaller", "options.json");
        if (!Directory.Exists(Path.GetDirectoryName(combined)))
            Directory.CreateDirectory(Path.GetDirectoryName(combined)!);    
        TeuJson.JsonTextWriter.WriteToFile(combined, json);
    }

    public static void Load() 
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var combined = Path.Combine(appData, "FortRiseInstaller", "options.json");

        if (!File.Exists(combined))
            return;
        var json = TeuJson.JsonTextReader.FromFile(combined);
        FNA = json["FNA"];
        DebugMode = json["DEBUG"];
    }

    public static async Task StateUnpatch() 
    {
        AnsiConsole.MarkupLine("Select a TowerFall directory to unpatch");
        var dialog = Dialog.FolderPicker();
        if (dialog.IsCancelled || dialog.IsError)  
        {
            AnsiConsole.MarkupLine("[red]Cancelled[/]");
            return;
        }
        var path = dialog.Path;
        AnsiConsole.WriteLine("Trying to find TowerFall executable in this directory.");
        if (!File.Exists(Path.Combine(path, "TowerFall.exe"))) 
        {
            AnsiConsole.MarkupLine("[underline][red]TowerFall not found! Aborting[/][/]");
            await Task.Delay(1000);
            return;
        }

        if (!AnsiConsole.Confirm($"""
        Are you sure you want to unpatch this directory?
        [yellow]{path}[/]
        """))
        {
            AnsiConsole.MarkupLine("[red]Cancelled[/]");
            return;
        }
        await Installer.Uninstall(path);
    }

    public static async Task StatePatch() 
    {
        AnsiConsole.MarkupLine("Select a TowerFall directory to patch");
        var dialog = Dialog.FolderPicker();
        if (dialog.IsCancelled || dialog.IsError)  
        {
            AnsiConsole.MarkupLine("[red]Cancelled[/]");
            return;
        }
        var path = dialog.Path;

        
        AnsiConsole.WriteLine("Trying to find TowerFall executable in this directory.");
        if (!File.Exists(Path.Combine(path, "TowerFall.exe"))) 
        {
            AnsiConsole.MarkupLine("[underline][red]TowerFall not found! Aborting[/][/]");
            await Task.Delay(1000);
            return;
        }
        AnsiConsole.MarkupLine("[underline][green]TowerFall found in this directory! [/][/]");

        AnsiConsole.WriteLine("Checking if the TowerFall has DarkWorld DLC");
        if (!Directory.Exists(Path.Combine(path, "DarkWorldContent"))) 
        {
            AnsiConsole.MarkupLine("[underline][yellow]WARNING: TowerFall does not have DarkWorld DLC.[/][/]");
            AnsiConsole.MarkupLine("[underline][yellow]Mods might not work properly on the game.[/][/]");
            if (!AnsiConsole.Confirm("Are you sure you want to proceed?")) 
            {
                AnsiConsole.MarkupLine("[red]Cancelled[/]");
                return;
            }
        }
        else 
            AnsiConsole.MarkupLine("[underline][green]TowerFall has DarkWorld DLC[/][/]");
        
        await Task.Delay(1000);

        if (!AnsiConsole.Confirm($"""
        Are you sure you want to patch this directory?
        [yellow]{path}[/]
        [gray]Make sure that this TowerFall has not been patched yet.[/]
        """))
        {
            AnsiConsole.MarkupLine("[red]Cancelled[/]");
            return;
        }
        await Installer.Install(path);
    }
#endif
}

#if ANSI
public enum MenuState 
{
    Patch,
    Unpatch,
    Settings,
    Quit
}

public enum SettingsState
{
    Debug,
    FNA,
    Quit
}
#endif