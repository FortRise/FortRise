#nullable enable
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

internal record struct BlacklistArcher(string ArcherID, bool IsVanilla);


internal sealed class FortRiseModuleSettings : ModuleSettings
{

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


    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Old Intro Logo", OldIntroLogo, (x) => OldIntroLogo = x);
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
        }, true);

        settings.CreateOnOff("ALLOW DINPUT CONTROLLER", AllowDInput, (x) =>
        {
            AllowDInput = x;
        }, true);

        settings.CreateOnOff("ALLOW RAW INPUT CONTROLLER", AllowRawInput, (x) =>
        {
            AllowRawInput = x;
        }, true);

        settings.CreateOnOff("RAW INPUT CORRELATE XINPUT", RawInputCorrelateXInput, (x) =>
        {
            RawInputCorrelateXInput = x;
        }, true);
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