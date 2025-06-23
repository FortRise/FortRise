using System;
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
    public patch_VersusModeButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom)
    {
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
            return;

        if (currentIndex < GameModeRegistry.VersusGameModes.Count + BuiltInModeCount - 1 && MenuInput.Right)
        {
            currentIndex++;
            if (currentIndex < 3)
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = false;
                MainMenu.VersusMatchSettings.Mode = (Modes)currentIndex + BuiltInModeCount;
            }
            else
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = true;
                var entry = GameModeRegistry.VersusGameModes[currentIndex - BuiltInModeCount];
                MainMenu.VersusMatchSettings.Mode = GameModeRegistry.GetGameModeModes(entry.Name);
                patch_MainMenu.VersusMatchSettings.CustomVersusModeName = entry.Name;
            }

            ModeSwitch?.Invoke();
            Sounds.ui_move2.Play(160f, 1f);
            iconWiggler.Start();
            base_OnConfirm();
            UpdateSides();
        }
        else if (currentIndex > 0 && MenuInput.Left)
        {
            currentIndex--;
            if (currentIndex < 3)
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = false;
                MainMenu.VersusMatchSettings.Mode = (Modes)currentIndex + BuiltInModeCount;
            }
            else
            {
                patch_MainMenu.VersusMatchSettings.IsCustom = true;
                var entry = GameModeRegistry.VersusGameModes[currentIndex - BuiltInModeCount];
                MainMenu.VersusMatchSettings.Mode = GameModeRegistry.GetGameModeModes(entry.Name);
                patch_MainMenu.VersusMatchSettings.CustomVersusModeName = entry.Name;
            }

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
        DrawRight = currentIndex < GameModeRegistry.VersusGameModes.Count + 3 - 1;
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
