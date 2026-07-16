using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_VersusModeButton : VersusModeButton
{
    public static event Action ModeSwitch;
    private Wiggler iconWiggler;
    internal static int currentIndex;
    private List<Modes> modeSelect;

    public patch_VersusModeButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom)
    {
    }

    [Prefix("System.Void .ctor(Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Vector2)")]
    public void Create()
    {
        modeSelect = [Modes.LastManStanding, Modes.HeadHunters, Modes.TeamDeathmatch];
        foreach (var gamemode in GameModeRegistry.VersusGameModes)
        {
            modeSelect.Add(gamemode.Modes);
        }
    }

    [MonoModLinkTo("TowerFall.BorderButton", "Update")]
    public void base_Update()
    {
        base.Update();
    }

    [MonoModLinkTo("TowerFall.BorderButton", "OnConfirm")]
    protected void base_OnConfirm()
    {
        base.OnConfirm();
    }

    [MonoModReplace]
    public override void Update()
    {
        const int BuiltInModeCount = 3;
        base_Update();

        string currentModeName = patch_MainMenu.VersusMatchSettings.CustomVersusModeName;

        if (!Selected)
        {
            return;
        }

        if (MenuInput.Right && currentIndex < modeSelect.Count - 1)
        {
            currentIndex += 1;
            var mode = modeSelect[currentIndex];

            if (mode is Modes.LastManStanding or Modes.TeamDeathmatch or Modes.HeadHunters)
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = false;
            }
            else
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = true;
                var entry = GameModeRegistry.VersusGameModes[currentIndex - BuiltInModeCount];
                patch_MainMenu.VersusMatchSettings.CustomVersusModeName = entry.Name;
            }

            MainMenu.VersusMatchSettings.Mode = mode;

            ModeSwitch?.Invoke();
            Sounds.ui_move2.Play(160f, 1f);
            iconWiggler.Start();
            base_OnConfirm();
            UpdateSides();
        }
        else if (MenuInput.Left && currentIndex > 0)
        {
            currentIndex -= 1;
            var mode = modeSelect[currentIndex];

            if (mode is Modes.LastManStanding or Modes.TeamDeathmatch or Modes.HeadHunters)
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = false;
            }
            else
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = true;
                var entry = GameModeRegistry.VersusGameModes[currentIndex - BuiltInModeCount];
                patch_MainMenu.VersusMatchSettings.CustomVersusModeName = entry.Name;
            }

            MainMenu.VersusMatchSettings.Mode = mode;

            ModeSwitch?.Invoke();
            Sounds.ui_move2.Play(160f, 1f);
            iconWiggler.Start();
            base_OnConfirm();
            UpdateSides();
        }
    }

    [MonoModReplace]
    private void UpdateSides()
    {
        DrawRight = currentIndex < modeSelect.Count - 1;
        DrawLeft = currentIndex != 0;
    }

    [MonoModReplace]
    public static string GetModeName(Modes mode)
    {
        switch (mode)
        {
        case Modes.LastManStanding:
            return "LAST MAN STANDING";
        case Modes.HeadHunters:
            return "HEADHUNTERS";
        case Modes.TeamDeathmatch:
            return "TEAM DEATHMATCH";
        case Modes.Warlord:
            return "WARLORD";
        default:
            if (GameModeRegistry.ModesToVersusGameMode.TryGetValue(mode, out var gamemode))
            {
                return gamemode.VersusGameMode.Name.ToUpperInvariant();
            }

            throw new Exception("Cannot get name for mode! This should only be used for Versus modes");
        }
    }

    [MonoModReplace]
    public static Subtexture GetModeIcon(Modes mode)
    {
        switch (mode)
        {
        case Modes.LastManStanding:
            return TFGame.MenuAtlas["gameModes/lastManStanding"];
        case Modes.HeadHunters:
            return TFGame.MenuAtlas["gameModes/headhunters"];
        case Modes.TeamDeathmatch:
            return TFGame.MenuAtlas["gameModes/teamDeathmatch"];
        case Modes.Warlord:
            return TFGame.MenuAtlas["gameModes/warlord"];
        default:
            if (GameModeRegistry.ModesToVersusGameMode.TryGetValue(mode, out var gamemode))
            {
                return gamemode.VersusGameMode.Icon.Subtexture;
            }
            throw new Exception("Cannot get icon for mode! This should only be used for Versus modes");
        }
    }
}
