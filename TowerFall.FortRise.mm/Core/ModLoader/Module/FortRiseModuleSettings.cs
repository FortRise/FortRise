#nullable enable
using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

internal record struct BlacklistArcher(string ArcherID, bool IsVanilla);


internal sealed class FortRiseModuleSettings : ModuleSettings
{
    public bool FixedTimeStep { get; set; }


    public bool OldIntroLogo { get; set; }
    public List<BlacklistArcher> BlacklistedArcher { get; set; } = [];
    public bool MusicMenuShowVanillaMusic { get; set; } = true;
    public bool MusicMenuShowModdedMusic { get; set; } = true;

    public string? MusicEnableMainMenu { get; set; } = null;
    public string? MusicEnableArchives { get; set; } = null;

    public bool AllowXInput { get; set; } = true;
    public bool AllowDInput { get; set; } = true;
    public bool AllowRawInput { get; set; } = false;
    public bool RawInputCorrelateXInput { get; set; } = true;
    public bool TripleBufferedVsync { get; set; } = false;


    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("USE FIXED TIME STEP", FixedTimeStep, (x) =>
        {
            FixedTimeStep = x;
            ((patch_Engine)Engine.Instance).EnableFixedTimeStep(FixedTimeStep);
        }, "MAKE THE GAME RUNS PRECISELY AT ITS TARGET FPS. MAKING THE PHYSICS AND PLAYER MOVEMENT ACCURATE");

        settings.CreateOnOff("Old Intro Logo", OldIntroLogo, (x) => OldIntroLogo = x, 
            "WILL USE VANILLA INTRO SPLASH SCENE");
        if (Engine.Instance.Scene is MainMenu menu)
        {
            settings.CreateButton("TOGGLE ARCHERS", () =>
            {
                menu.State = ModRegisters.MenuState<UIArcherBlacklist>();
            });

            settings.CreateButton("MUSIC LIST", () =>
            {
                menu.State = ModRegisters.MenuState<UIMusicList>();
            });
        }

        settings.CreateOnOff("ALLOW XINPUT CONTROLLER", AllowXInput, (x) =>
        {
            AllowXInput = x;
        }, true, "ALLOWS THE GAME TO DETECT XINPUT CONTROLLERS LIKE XBOX CONTROLLERS");

        settings.CreateOnOff("ALLOW DINPUT CONTROLLER", AllowDInput, (x) =>
        {
            AllowDInput = x;
        }, true, "ALLOWS THE GAME TO DETECT DINPUT CONTROLLERS. DISABLING THIS WILL FORCE AN ALTERNATIVE TO BE USED LIKE XINPUT");

        settings.CreateOnOff("ALLOW RAW INPUT CONTROLLER", AllowRawInput, (x) =>
        {
            AllowRawInput = x;
        }, true, "ALLOWS RAW INPUT TO HANDLE XINPUT DEVICES FOR BETTER COMPATIBILITY");

        settings.CreateOnOff("RAW INPUT CORRELATE XINPUT", RawInputCorrelateXInput, (x) =>
        {
            RawInputCorrelateXInput = x;
        }, true, "RAW INPUT CAN STILL USE XINPUT API TO HANDLE EXTRA FEATURES LIKE RUMBLE AND BETTER TRIGGER AXES");
    }

    public override void OnVerify()
    {
        List<BlacklistArcher> toRemove = [];

        for (int i = 0; i < BlacklistedArcher.Count; i++)
        {
            var blacklist = BlacklistedArcher[i];
            if (blacklist.IsVanilla) // we don't need to verify if its vanilla
            {
                continue;
            }

            if (ArcherRegistry.GetArcherEntry(blacklist.ArcherID) is null)
            {
                toRemove.Add(blacklist);
            }
        }

        foreach (var removal in toRemove)
        {
            BlacklistedArcher.Remove(removal);
        }
    }
}
