#define FORTMODULE

using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_TFGame : TFGame
{
    public patch_TFGame(bool noIntro) : base(noIntro)
    {
    }

#if FORTMODULE
    public extern static void orig_Main(string[] args);
    public static void Main(string[] args) 
    {
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

    protected extern void orig_LoadContent();

    protected override void LoadContent()
    {
        FortRise.RiseCore.LoadContent();
        orig_LoadContent();
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