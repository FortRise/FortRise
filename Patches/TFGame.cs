#pragma warning disable CS0626
#pragma warning disable CS0108
namespace TowerFall;

public class patch_TFGame : TFGame
{
    public patch_TFGame(bool noIntro) : base(noIntro)
    {
    }

    protected extern void orig_Initialize();

    protected override void Initialize() 
    {
        FortRise.RiseCore.Initialize();
        orig_Initialize();
    }

    public static extern void orig_Load();

    public static void Load() 
    {
        FortRise.RiseCore.ModuleStart();
        orig_Load();
    }

    protected extern void orig_LoadContent();

    protected override void LoadContent()
    {
        FortRise.RiseCore.ModuleEnd();
        orig_LoadContent();
    }

    protected extern void orig_UnloadContent();

    protected override void UnloadContent()
    {
        FortRise.RiseCore.ModuleEnd();
        orig_UnloadContent();
    }
}