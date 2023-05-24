using System;
using System.Collections;
using System.Collections.Generic;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldControl : DarkWorldControl 
{
    private List<Pickups> midwayTreasure;
    private bool bossMode;
    [PostPatchEnableTempVariant]
    public static void ActivateTempVariants(Level level, patch_DarkWorldTowerData.patch_LevelData levelData) 
    {
        patch_MatchVariants matchVariant = (level.Session.MatchSettings.Variants as patch_MatchVariants);
        if (levelData.ActiveVariant.CustomVariants != null) 
        {
            foreach (var variant in matchVariant.CustomVariants) 
            {
                var key = variant.Key;
                if (!levelData.ActiveVariant.CustomVariants.TryGetValue(key, out var val)) 
                {
                    continue;
                }
                if (!variant.Value && val) 
                {
                    TempVariantHolder.TempCustom[key] = true;
                    matchVariant.CustomVariants[key].Value = true;
                }
            }
        }

        JumpLanding();
    }


    [PostPatchDisableTempVariant]
    public static void DisableTempVariants(Level level) 
    {
        patch_MatchVariants matchVariant = (level.Session.MatchSettings.Variants as patch_MatchVariants);
        var modified = new List<string>();
        foreach (var variant in TempVariantHolder.TempCustom) 
        {
            var variantKey = variant.Key;
            var variantVal = variant.Value;
            if (variantVal) 
            {
                modified.Add(variantKey);
                matchVariant.GetCustomVariant(variantKey).Value = false;
            }
        }
        foreach (var modifiedVariant in modified) 
        {
            TempVariantHolder.TempCustom[modifiedVariant] = false;
        }
        JumpLanding();
    }

    // Doing this will prevent the instruction jumping on a void
    private static void JumpLanding() {}

    private extern IEnumerator orig_LevelSequence();

    [MonoModReplace]
    private void StartMusic()
    {
        if (bossMode)
            return;
        
        var levelSession = Level.Session;
        var themeMusic = Level.Session.MatchSettings.LevelSystem.Theme.Music;
        levelSession.SongTimer = 0;
        if (themeMusic.Contains("custom:", StringComparison.OrdinalIgnoreCase))
        {
            var storedDirectory = patch_GameData.AdventureWorldTowers[levelSession.MatchSettings.LevelSystem.ID.X].StoredDirectory;

            var path = PathUtils.CombinePrefixPath(
                themeMusic, 
                storedDirectory,
                "custom:");
            Music.Play(path);
            return;
        }
        Music.Play(themeMusic);
    }

    private IEnumerator LevelSequence() 
    {
        patch_DarkWorldControl.DisableTempVariants(Level);
        var matchSettings = Level.Session.MatchSettings;

        patch_DarkWorldLevelSystem darkWorld = matchSettings.LevelSystem as patch_DarkWorldLevelSystem;
        var levelData = darkWorld.GetLevelData(matchSettings.DarkWorldDifficulty, Level.Session.RoundIndex);
        patch_DarkWorldControl.ActivateTempVariants(Level, levelData);
        if (patch_SaveData.AdventureActive) 
        {
            if (matchSettings.Variants.AlwaysDark)
            {
                Level.OrbLogic.DoDarkOrb();
            }
            if (matchSettings.Variants.SlowTime)
            {
                Level.OrbLogic.DoTimeOrb(delay: true);
            }
            if (matchSettings.Variants.AlwaysLava)
            {
                Level.OrbLogic.DoLavaVariant();
            }
            if (matchSettings.Variants.AlwaysScrolling)
            {
                Level.OrbLogic.StartScroll();
            }
        }

        yield return orig_LevelSequence();
    }

    private void DoBossLevelSetup(int bossID, List<Pickups> treasure) 
    {
        //FIXME Will probably not do this soon
		var levelData = (Level.Session.MatchSettings.LevelSystem as DarkWorldLevelSystem).GetLevelData(base.Level.Session.MatchSettings.DarkWorldDifficulty, base.Level.Session.RoundIndex) as patch_DarkWorldTowerData.patch_LevelData;

        if (!string.IsNullOrEmpty(levelData.CustomBossName)) 
        {
            if (RiseCore.DarkWorldBossLoader.TryGetValue(levelData.CustomBossName, out var val)) 
            {
                this.midwayTreasure = treasure;
                int darkWorldDifficulty = (int)base.Level.Session.MatchSettings.DarkWorldDifficulty;
                Level.Add(val(darkWorldDifficulty));
                return;
            }
            Logger.Error($"Failed to spawn boss type name: {levelData.CustomBossName}. Falling back to Amaranth Boss");
        }
        orig_BossLevelSetup(bossID, treasure);
    }

    private extern void orig_BossLevelSetup(int bossID, List<Pickups> treasure);
}