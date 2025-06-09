using System;
using FortRise.Entities;

namespace FortRise;

public sealed class FortRiseModule : FortModule
{
    public static FortRiseModule Instance;

    public override Type SaveDataType => typeof(FortRiseModuleSaveData);
    public static FortRiseModuleSaveData SaveData => (FortRiseModuleSaveData)Instance.InternalSaveData;

    public FortRiseModule()
    {
        Instance = this;

        // Internal things, don't try this in your own mods.
        Meta = new ModuleMetadata() {
            Name = "FortRise",
            Version = RiseCore.FortRiseVersion,
        };
    }

    public override void Initialize()
    {
        Registry.Enemies.RegisterEnemy("SlimeS", new() 
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.SlimeS
        });

        Registry.Enemies.RegisterEnemy("BlueSlimeS", new() 
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.BlueSlimeS
        });

        Registry.Enemies.RegisterEnemy("RedSlimeS", new() 
        {
            Name = "Slime Shield",
            Loader = ShieldSlime.RedSlimeS
        });
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
    }
}