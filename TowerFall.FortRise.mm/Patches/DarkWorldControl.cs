using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework.Audio;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldControl : DarkWorldControl 
{
    private bool bossMode;
    private SoundEffectInstance customMusic;
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
                    Logger.Log(key);
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

    [PatchDarkWorldControlLevelSequence]
    private extern IEnumerator orig_LevelSequence();

    [MonoModReplace]
    private void StartMusic()
    {
        if (bossMode)
            return;
        
        if (SoundHelper.StoredInstance.TryGetValue("CustomDarkWorldMusic", out customMusic)) 
        {
            if (customMusic.State == SoundState.Playing)
                return;
            SoundHelper.PlayMusic(customMusic);
            return;
        }
        
        var levelSession = Level.Session;
        var themeMusic = Level.Session.MatchSettings.LevelSystem.Theme.Music;
        levelSession.SongTimer = 0;
        if (themeMusic.Contains("custom:", StringComparison.OrdinalIgnoreCase) 
            && TryPlayCustomMusic(themeMusic.AsSpan()))
        {
            SoundHelper.PlayMusic(customMusic);
            return;
        }
        Music.Play(themeMusic);
    }

    private bool TryPlayCustomMusic(ReadOnlySpan<char> themeMusic) 
    {
        if (customMusic == null)
        {
            var themeSpan = themeMusic.Slice(7);
            var localPath = themeSpan.ToString();
            var path =
                Path.Combine(
                    patch_GameData.AdventureWorldTowers[Level.Session.MatchSettings.LevelSystem.ID.X].StoredDirectory,
                    localPath
                );
            if (!File.Exists(path))
                return false;
            
            SoundHelper.PathToSound(path, out customMusic);
            if (!SoundHelper.StoredInstance.ContainsKey("CustomDarkWorldMusic"))
                SoundHelper.StoredInstance.Add("CustomDarkWorldMusic", customMusic);
        }
        return true;
    }

    private void StopMusic() 
    {
        Music.Stop();
        customMusic?.Stop();
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
}