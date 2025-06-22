using FortRise.Entities;
using Microsoft.Extensions.Logging;

namespace FortRise;

public sealed class FortRiseModule : Mod
{
    public static FortRiseModule Instance;
    public static FortRiseModuleSaveData SaveData => Instance.GetSaveData<FortRiseModuleSaveData>();

    public FortRiseModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        // Internal things, don't try this in your own mods.
        Meta = new ModuleMetadata()
        {
            Name = "FortRise",
            Version = RiseCore.FortRiseVersion,
        };

        Context.Registry.Enemies.RegisterEnemy("SlimeS", new()
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.SlimeS
        });

        Context.Registry.Enemies.RegisterEnemy("BlueSlimeS", new()
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.BlueSlimeS
        });

        Context.Registry.Enemies.RegisterEnemy("RedSlimeS", new()
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.RedSlimeS
        });
    }

    public override ModuleSaveData CreateSaveData()
    {
        return new FortRiseModuleSaveData();
    }
}