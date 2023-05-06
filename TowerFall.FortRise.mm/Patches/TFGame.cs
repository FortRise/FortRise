#define FORTMODULE

namespace TowerFall;

public class patch_TFGame : TFGame
{
    public patch_TFGame(bool noIntro) : base(noIntro)
    {
    }

#if FORTMODULE
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
}