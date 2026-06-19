using System;
using System.Runtime.CompilerServices;
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
    public static Subtexture PresetAddIcon;
    public static Subtexture PresetCustomIcon;
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

        Context.Events.OnBeforeSaveSaveData += OnSave;
    }

    public override ModuleSaveData CreateSaveData()
    {
        return new FortRiseModuleSaveData();
    }

    public override ModuleSettings CreateSettings()
    {
        return new FortRiseModuleSettings();
    }


    public static void OnSave(object sender, BeforeSaveSaveDataEventArgs e)
    {
        // Backup the data from savedata to mod if possible
        // this should only be possible if the mod data exists on the TowerFall savedata side
        for (int i = 0; i < TowerFall.SaveData.Instance.Quest.Towers.Length; i += 1)
        {
            TowerFall.Patching.QuestTowerStats tower = TowerFall.SaveData.Instance.Quest.Towers[i] as TowerFall.Patching.QuestTowerStats;
            if (tower is null || tower.LevelID is null)
            {
                continue;
            }

            if (!tower.LevelID.Contains('/')) // only selects the level with slash for backup
            {
                continue;
            }


            SaveData.AdventureQuest.Towers[tower.LevelID] = tower;
        }

        for (int i = 0; i < TowerFall.SaveData.Instance.DarkWorld.Towers.Length; i += 1)
        {
            TowerFall.Patching.DarkWorldTowerStats tower = TowerFall.SaveData.Instance.DarkWorld.Towers[i] as TowerFall.Patching.DarkWorldTowerStats;
            if (tower is null || tower.LevelID is null)
            {
                continue;
            }

            if (!tower.LevelID.Contains('/')) // only selects the level with slash for backup
            {
                continue;
            }

            SaveData.AdventureWorld.Towers[tower.LevelID] = tower;
        }

        for (int i = 0; i < TowerFall.SaveData.Instance.Trials.Levels.Length; i += 1)
        {
            var level = TowerFall.SaveData.Instance.Trials.Levels[i];
            for (int j = 0; j < level.Length; j += 1)
            {
                var stats = Unsafe.BitCast<TowerFall.TrialsLevelStats, TowerFall.Patching.TrialsLevelStats>(TowerFall.SaveData.Instance.Trials.Levels[i][j]);

                if (stats.LevelID is null)
                {
                    continue;
                }

                if (!stats.LevelID.Contains('/')) // only selects the level with slash for backup
                {
                    continue;
                }

                SaveData.AdventureTrials.Towers[stats.LevelID] = stats;
            }
        }
    }
}
