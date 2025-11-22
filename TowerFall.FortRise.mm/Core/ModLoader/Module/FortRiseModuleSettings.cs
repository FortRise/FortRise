using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

internal record struct BlacklistArcher(string ArcherID, bool IsVanilla);

internal sealed class FortRiseModuleSettings : ModuleSettings
{
    public bool OldIntroLogo { get; set; }
    public List<BlacklistArcher> BlacklistedArcher { get; set; } = [];

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Old Intro Logo", OldIntroLogo, (x) => OldIntroLogo = x);
        if (Engine.Instance.Scene is MainMenu menu)
        {
            settings.CreateButton("TOGGLE ARCHERS", () =>
            {
                menu.State = ModRegisters.MenuState<UIArcherBlacklist>();
            });
        }
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