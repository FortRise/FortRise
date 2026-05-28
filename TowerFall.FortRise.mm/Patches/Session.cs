using System;
using System.Collections.Generic;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall 
{
    public class patch_Session : Session
    {
        private patch_DarkWorldSessionState DarkWorldState;
        [MonoModIgnore]
        public TreasureSpawner TreasureSpawner { get; private set; }
        private List<Variant> temporarilyActivatedVariants;

        public patch_Session(MatchSettings settings) : base(settings)
        {
        }


        [Prefix("System.Void .ctor(TowerFall.MatchSettings)")]
        private static void ctor_Prefix(patch_Session __instance)
        {
            __instance.temporarilyActivatedVariants = new();
        }

        public patch_MatchSettings MatchSettings;

        private void ActivateTempVariants(Level level, patch_DarkWorldTowerData.patch_LevelData levelData)
        {
            if (levelData.Variants == null)
            {
                return;
            }
            patch_MatchVariants matchVariant = (level.Session.MatchSettings.Variants as patch_MatchVariants);
            foreach (var variant in levelData.Variants)
            {
                if (!matchVariant.TryGetCustomVariant(variant, out var v))
                {
                    Logger.Error($"Variant Name: '{variant}' cannot be found on the registry.");
                    continue;
                }
                if (v.CoOpValue != 0)
                {
                    if (v.Value)
                    {
                        continue;
                    }
                    v.Value = true;
                    temporarilyActivatedVariants.Add(v);
                }
            }
        }

        public void DisableTempVariants(Level level)
        {
            foreach (var temp in temporarilyActivatedVariants)
            {
                temp.Value = false;
            }
            temporarilyActivatedVariants.Clear();
        }

        [Postfix(nameof(LevelLoadStart))]
        private void LevelLoadStart_Postfix(Level level)
        {
            var matchSettings = MatchSettings;

            if (matchSettings.LevelSystem is patch_DarkWorldLevelSystem darkWorld)
            {
                DisableTempVariants(CurrentLevel);

                var levelData = darkWorld.GetLevelData(matchSettings.DarkWorldDifficulty, RoundIndex);
                ActivateTempVariants(CurrentLevel, levelData);
            }

            var levelType = this.IsOfficialTowerSet ? "vanilla" : "modded";
            level.AssignTag(levelType);
            var set = this.TowerSet;
            level.AssignTag("set=" + set);
            level.AssignTag("theme=" + ((patch_TowerTheme)(matchSettings.LevelSystem.Theme)).ID);
            var levelSystem = matchSettings.LevelSystem;
            switch (levelSystem) 
            {
            case DarkWorldLevelSystem dwSystem:
                level.AssignTag("level=" + dwSystem.DarkWorldTowerData.LevelID);
                level.AssignTag("darkworld");
                break;
            case TrialsLevelSystem lSystem:
                level.AssignTag("level=" + lSystem.TrialsLevelData.LevelID);
                level.AssignTag("trials");
                break;
            case QuestLevelSystem qSystem:
                level.AssignTag("level=" + qSystem.QuestTowerData.LevelID);
                level.AssignTag("quest");
                break;
            case VersusLevelSystem vSystem:
                level.AssignTag("level=" + vSystem.VersusTowerData.LevelID);
                level.AssignTag("versus");
                break;
            }
        }


        [MonoModPatch("StartGame")]
        [MonoModIfFlag("Steamworks")]
        public void StartGame_Steam() 
        {
			if (!MatchSettings.SoloMode)
			{
				GameStats stats = SaveData.Instance.Stats;
				int num = stats.MatchesPlayed;
				stats.MatchesPlayed = num + 1;
                if (this.IsOfficialTowerSet)
                    stats.VersusTowerPlays[MatchSettings.LevelSystem.ID.X] += 1UL;
				if (MatchSettings.RandomVersusTower)
				{
					num = stats.VersusRandomPlays;
					stats.VersusRandomPlays = num + 1;
				}
				else if (this.IsOfficialTowerSet)
				{
					stats.RegisterVersusTowerSelection(MatchSettings.LevelSystem.ID.X);
				}
				SessionStats.MatchesPlayed++;
                if (MatchSettings.IsCustom) 
                {
                    var gamemode = MatchSettings.CustomVersusGameMode;
                    if (gamemode != null)
                    {
                        gamemode.OnStartGame(this);
                    }
                }
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialTowerSet)
                {
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                }
                else 
                {
                    patch_DarkWorldTowerData adventureTower = (patch_DarkWorldTowerData)GameData.DarkWorldTowers[MatchSettings.LevelSystem.ID.X];
                    FortRiseModule.SaveData.AdventureWorld.AddOrGet(adventureTower.LevelID).Attempts += 1;
                    if (adventureTower.StartingLives >= 0)
                    {
                        DarkWorldState.ExtraLives = adventureTower.StartingLives;
                    }
                }
			}
			TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
			Engine.Instance.Scene = new LevelLoaderXML(this);
        }

        [MonoModPatch("StartGame")]
        [MonoModIfFlag("NoLauncher")]
        public void StartGame_NoLauncher() 
        {
			if (!MatchSettings.SoloMode)
			{
                if (this.IsOfficialTowerSet) 
                {
                    GameStats stats = SaveData.Instance.Stats;
                    stats.VersusTowerPlays[MatchSettings.LevelSystem.ID.X] += 1UL;
                }
				SessionStats.MatchesPlayed++;
                if (MatchSettings.IsCustom) 
                {
                    var gamemode = MatchSettings.CustomVersusGameMode;
                    if (gamemode != null)
                    {
                        gamemode.OnStartGame(this);
                    }
                }
			}
			if (MatchSettings.Mode == patch_Modes.DarkWorld)
			{
				DarkWorldState = new patch_DarkWorldSessionState(this);
                if (this.IsOfficialTowerSet)
                {
                    SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
                }
                else 
                {
                    patch_DarkWorldTowerData adventureTower = (patch_DarkWorldTowerData)GameData.DarkWorldTowers[MatchSettings.LevelSystem.ID.X];
                    FortRiseModule.SaveData.AdventureWorld.AddOrGet(adventureTower.LevelID).Attempts += 1;
                    if (adventureTower.StartingLives >= 0)
                    {
                        DarkWorldState.ExtraLives = adventureTower.StartingLives;
                    }
                }
			}
			TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
			Engine.Instance.Scene = new LevelLoaderXML(this);
        }
    }

    public static class SessionExt 
    {
        extension(Session session)
        {
            public string TowerSet => ((patch_Session)session).MatchSettings.LevelSystem switch
            {
                VersusLevelSystem system => GameData.VersusTowers[system.VersusTowerData.ID.X].TowerSet,
                QuestLevelSystem system => GameData.QuestLevels[system.QuestTowerData.ID.X].TowerSet,
                DarkWorldLevelSystem system => GameData.DarkWorldTowers[system.DarkWorldTowerData.ID.X].TowerSet,
                TrialsLevelSystem system => GameData.TrialsLevels[system.TrialsLevelData.ID.X, system.TrialsLevelData.ID.Y].TowerSet,
                _ => "TowerFall"
            };

            public bool IsOfficialTowerSet => ((patch_Session)session).MatchSettings.LevelSystem switch
            {
                VersusLevelSystem system => GameData.VersusTowers[system.VersusTowerData.ID.X].IsOfficialTowerSet,
                QuestLevelSystem system => GameData.QuestLevels[system.QuestTowerData.ID.X].IsOfficialTowerSet,
                DarkWorldLevelSystem system => GameData.DarkWorldTowers[system.DarkWorldTowerData.ID.X].IsOfficialTowerSet,
                TrialsLevelSystem system => GameData.TrialsLevels[system.TrialsLevelData.ID.X, system.TrialsLevelData.ID.Y].IsOfficialTowerSet,
                _ => true
            };
        }


        [Obsolete("Session.TowerSet can no longer be set")]
        public static void SetLevelSet(this Session session, string levelSet)
        {
        }

        [Obsolete("Use 'Session.TowerSet' instead")]
        public static string GetLevelSet(this Session session) 
        {
            return session.TowerSet;
        }

        [Obsolete("Use 'Session.IsOfficialTowerSet' instead")]
        public static bool IsOfficialLevelSet(this Session session) 
        {
            return session.IsOfficialTowerSet;
        }
    }
}