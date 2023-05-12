#define FORTMODULE

using System.IO;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace TowerFall;

public class patch_TFGame : TFGame
{
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
                        break;
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
            Logger.AttachConsole(RiseCore.ConsoleAttachment());
        }
        orig_Main(args);
        Logger.DetachConsole();
        Logger.WriteToFile("fortRiseLog.txt");
    }

    protected extern void orig_Initialize();

    protected override void Initialize() 
    {
        FortRise.RiseCore.ModuleStart();
        FortRise.RiseCore.Initialize();
        orig_Initialize();
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
        RiseCore.Invoke_BeforeUpdate(gameTime);
        RiseCore.Invoke_Update(gameTime);
        orig_Update(gameTime);
        RiseCore.Invoke_AfterUpdate(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RiseCore.Invoke_BeforeRender(Monocle.Draw.SpriteBatch);
        RiseCore.Invoke_Render(Monocle.Draw.SpriteBatch);
        base.Draw(gameTime);
        RiseCore.Invoke_AfterRender(Monocle.Draw.SpriteBatch);
    }

}