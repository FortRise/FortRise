#define FORTMODULE

using System;
using System.IO;
using System.Runtime.InteropServices;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace TowerFall;

internal static class NativeMethods 
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDefaultDllDirectories(int directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern void AddDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string lpPathName);
}

public class patch_TFGame : TFGame
{
    private const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;


    private bool noIntro;
    public patch_TFGame(bool noIntro) : base(noIntro)
    {
    }

    public extern void orig_ctor(bool noIntro);

    [MonoModConstructor]
    public void ctor(bool noIntro) 
    {
        orig_ctor(noIntro);
        this.noIntro = RiseCore.DebugMode;
    }

#if FORTMODULE
    public extern static void orig_Main(string[] args);
    public static void Main(string[] args) 
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) 
        {
            try 
            {
                NativeMethods.SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
                NativeMethods.AddDllDirectory(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Environment.Is64BitProcess ? "x64" : "x86"
                ));
            }
            catch 
            {
                NativeMethods.SetDllDirectory(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Environment.Is64BitProcess ? "x64" : "x86"
                ));
            }
        }
        var patchFile = "PatchVersion.txt";
        if (File.Exists(patchFile)) 
        {
            try 
            {
                using var fs = File.OpenRead(patchFile);
                using TextReader reader = new StreamReader(fs);
                string readed;
                while ((readed = reader.ReadLine()) != null) 
                {
                    if (readed.Contains("Debug")) 
                    {
                        var debugMode = readed.Split(':')[1].Trim();
                        RiseCore.DebugMode = bool.Parse(debugMode);
                    }
                    if (readed.Contains("Installer")) 
                    {
                        var version = readed.Split(':')[1].Trim();
                        RiseCore.FortRiseVersion = new System.Version(version);
                    }
                }
            }
            catch 
            {
                Logger.Log("Unable to load PatchVersion.txt, Debug Mode Proceed", Logger.LogLevel.Warning);
                Logger.Log("Please report this bug", Logger.LogLevel.Warning);
                RiseCore.DebugMode = true;
            }

        }
        if (RiseCore.DebugMode) 
        {
            var detourModManager = new DetourModManager();
            detourModManager.OnILHook += (assembly, source, dest) => 
            {
                object obj = dest.Target;
                RiseCore.DetourLogs.Add($"ILHook from {assembly.GetName().Name}: {source.GetID()} :: {dest.Method?.GetID() ?? "??"}{(obj == null ? "" : $"(object: {obj})")}");
            };
            detourModManager.OnHook += (assembly, source, dest, obj) => 
            {
                RiseCore.DetourLogs.Add($"Hook from {assembly.GetName().Name}: {source.GetID()} :: {dest.GetID()}{(obj == null ? "" : $"(object: {obj})")}");
            };
            try 
            {
                Logger.AttachConsole(RiseCore.ConsoleAttachment());
            }
            catch (Exception e) 
            {
                Logger.Error("Failed to attach console.");
                Logger.Error(e.ToString());
            }
        }
        orig_Main(args);
        Logger.DetachConsole();
        Logger.WriteToFile("fortRiseLog.txt");
    }

    protected extern void orig_Initialize();

    protected override void Initialize() 
    {
        FortRise.RiseCore.ModuleStart();
        FortRise.RiseCore.Events.Invoke_OnPreInitialize();
        FortRise.RiseCore.Initialize();
        orig_Initialize();
        FortRise.RiseCore.Events.Invoke_OnPostInitialize();
        patch_Arrow.ExtendArrows();
        FortRise.RiseCore.LogAllTypes();
    }

    protected extern void orig_UnloadContent();

    protected override void UnloadContent()
    {
        FortRise.RiseCore.ModuleEnd();
        orig_UnloadContent();
    }
#endif

    protected extern void orig_Update(GameTime gameTime);

    protected override void Update(GameTime gameTime)
    {
        RiseCore.Events.Invoke_BeforeUpdate(gameTime);
        RiseCore.Events.Invoke_Update(gameTime);
        orig_Update(gameTime);
        RiseCore.Events.Invoke_AfterUpdate(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RiseCore.Events.Invoke_BeforeRender(Monocle.Draw.SpriteBatch);
        RiseCore.Events.Invoke_Render(Monocle.Draw.SpriteBatch);
        base.Draw(gameTime);
        RiseCore.Events.Invoke_AfterRender(Monocle.Draw.SpriteBatch);
    }

}