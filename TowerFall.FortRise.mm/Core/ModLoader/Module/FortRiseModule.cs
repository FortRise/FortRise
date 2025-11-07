using FortRise.Entities;
using Microsoft.Extensions.Logging;
using Monocle;
using TowerFall;

namespace FortRise;

public sealed class FortRiseModule : Mod
{
    public static FortRiseModule Instance;
    public static FortRiseModuleSaveData SaveData => Instance.GetSaveData<FortRiseModuleSaveData>();
    public static Subtexture FortRiseIcon;
    internal static FortRiseModuleSettings Settings => Instance.GetSettings<FortRiseModuleSettings>();

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

        Context.Registry.QuestEvents.RegisterQuestEvent("MiasmaWall", new()
        {
            Appear = (level) => level.Add(new Miasma(Miasma.Modes.CataclysmBoss)),
            Disappear = (level) => level.Layers[0].GetFirst<Miasma>()?.Dissipate()
        });

        Context.Registry.QuestEvents.RegisterQuestEvent("MiasmaWallMoving", new()
        {
            Appear = (level) => level.Add(new Miasma(Miasma.Modes.TheAmaranthBoss)),
            Disappear = (level) => level.Layers[0].GetFirst<Miasma>()?.Dissipate()
        });

        Context.Registry.QuestEvents.RegisterQuestEvent("MiasmaBottom", new()
        {
            Appear = (level) => level.Add(new BottomMiasma(BottomMiasma.Modes.DreadwoodBoss)),
            Disappear = (level) => level.Layers[0].GetFirst<BottomMiasma>()?.Dissipate()
        });
    }

    public override ModuleSaveData CreateSaveData()
    {
        return new FortRiseModuleSaveData();
    }

    public override ModuleSettings CreateSettings()
    {
        return new FortRiseModuleSettings();
    }
}
