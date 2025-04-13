using System;
using FortRise.Adventure.Entities;

namespace FortRise.Adventure;

public sealed class AdventureModule : FortModule
{
    public static AdventureModule Instance;

    public override Type SaveDataType => typeof(AdventureModuleSaveData);
    public static AdventureModuleSaveData SaveData => (AdventureModuleSaveData)Instance.InternalSaveData;

    public AdventureModule()
    {
        Instance = this;

        // Internal things, don't try this in your own mods.
        Meta = new ModuleMetadata() {
            Name = "Adventure",
            Author = "FortRise",
            Version = new SemanticVersion("4.0.0"),
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