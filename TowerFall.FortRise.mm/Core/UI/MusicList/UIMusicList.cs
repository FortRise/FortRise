using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIMusicList(MainMenu main) : CustomMenuState(main)
{
    private readonly string[] vanillaMusic = [
        "Title",
        "TheTale",
        "TheArchives",
        "ChipTitle",
        "AltTitle",

        "SacredGround",
        "TwilightSpire",
        "Backfire",
        "Flight",
        "Mirage",
        "Thornwood",
        "FrostfangKeep",
        "KingsCourt",
        "Boss",

        "SunkenCity",
        "Moonstone",
        "TowerForge",
        "Ascension",
        "Gauntlet",

        "TheAmaranth",
        "Dreadwood",
        "Darkfang",
        "Cataclysm",
        "DarkBoss",

        "VictoryTeam",
        "VictoryWhite",
        "VictoryPink",
        "VictoryOrange",
        "VictoryGreen",
        "VictoryBlue",
        "VictoryYellow",
        "VictoryCyan",
        "VictoryPurple",
        "VictoryOrangeAlt",
        "VictoryWhiteAlt",
        "VictoryRed",
        "VictoryKyle",

        "UnlockTwilightSpire",
        "UnlockMoonstone",
        "UnlockSunkenCity",
        "UnlockTowerForge",
        "UnlockAscension",
        "WhiteArcherReveal",

        "BattleCyan",
        "GameOver",
    ];
    private Entity guideEntity;

    public override void Create()
    {
        List<OptionsButton> optionsButton = [];
        var showVanillaMusicOptions = new OptionsButton("SHOW VANILLA MUSIC");
        showVanillaMusicOptions.SetCallbacks(() =>
        {
            showVanillaMusicOptions.State = FortRiseModule.Settings.MusicMenuShowVanillaMusic ? "ON" : "OFF";
        }, null, null, () => 
        {
            FortRiseModule.Settings.MusicMenuShowVanillaMusic = !FortRiseModule.Settings.MusicMenuShowVanillaMusic;
            return FortRiseModule.Settings.MusicMenuShowVanillaMusic;
        });

        var showModdedMusicOptions = new OptionsButton("SHOW MODDED MUSIC");
        showModdedMusicOptions.SetCallbacks(() =>
        {
            showModdedMusicOptions.State = FortRiseModule.Settings.MusicMenuShowModdedMusic ? "ON" : "OFF";
        }, null, null, () => 
        {
            FortRiseModule.Settings.MusicMenuShowModdedMusic = !FortRiseModule.Settings.MusicMenuShowModdedMusic;
            return FortRiseModule.Settings.MusicMenuShowModdedMusic;
        });

        optionsButton.Add(
            showVanillaMusicOptions
        );

        optionsButton.Add(
            showModdedMusicOptions
        );

        if (FortRiseModule.Settings.MusicMenuShowVanillaMusic)
        {
            foreach (var music in vanillaMusic)
            {
                CreateMusicOptionButton(music);
            }
        }

        if (FortRiseModule.Settings.MusicMenuShowModdedMusic)
        {
            // modded
            foreach (var music in MusicRegistry.MusicEntries)
            {
                CreateMusicOptionButton(music.Name);
            }
        }

        InitOptions(optionsButton, out int offset);

        ((patch_MainMenu)Main).ToStartSelected = optionsButton[0];
        Main.MaxUICameraY = offset;
        Main.Add(optionsButton);
        Main.BackState = MainMenu.MenuState.Options;

        guideEntity = new Entity(0);
        MenuButtonGuide setGuide = new MenuButtonGuide(0, MenuButtonGuide.ButtonModes.Alt, "SET MUSIC");
        Main.Add(guideEntity);
        guideEntity.Add(setGuide);

        void CreateMusicOptionButton(string name)
        {
            var optionButton = new MusicOptionsButton(name.ToUpperInvariant());
            optionButton.OnAlt = () =>
            {
                optionButton.Selected = false;
                UIModal modal = new UIModal(0);
                modal.AddFiller("SELECT AN AREA");

                modal.AddItem(string.IsNullOrEmpty(FortRiseModule.Settings.MusicEnableMainMenu) 
                    ? "MAIN MENU"
                    : "MAIN MENU: " + FortRiseModule.Settings.MusicEnableMainMenu.ToUpperInvariant(), () => 
                { 
                    FortRiseModule.Settings.MusicEnableMainMenu = name;
                    Music.Play(name);
                    Close(); 
                });

                modal.AddItem(string.IsNullOrEmpty(FortRiseModule.Settings.MusicEnableArchives) 
                    ? "ARCHIVES"
                    : "ARCHIVES: " + FortRiseModule.Settings.MusicEnableArchives.ToUpperInvariant(), () =>
                {
                    FortRiseModule.Settings.MusicEnableArchives = name;
                    Close();
                });

                modal.AddItem("CLEAR", () =>
                {
                    if (FortRiseModule.Settings.MusicEnableMainMenu == name)
                    {
                        FortRiseModule.Settings.MusicEnableMainMenu = null;
                    }

                    if (FortRiseModule.Settings.MusicEnableArchives == name)
                    {
                        FortRiseModule.Settings.MusicEnableArchives = null;
                    }
                    
                    Close();
                });

                modal.AddItem("CANCEL", () =>
                {
                    Close();
                });
                Main.Add(modal);

                void Close()
                {
                    optionButton.Selected = true;
                }
            };   

            optionButton.SetCallbacks(() => { optionButton.State = ""; }, null, null, () =>
            {
                Music.Stop();
                Music.Play(name);
                return false;
            });
            optionsButton.Add(optionButton);
        }
    }

    public override void Destroy()
    {
        Main.Remove(guideEntity);
    }

    private static void InitOptions(List<OptionsButton> buttons, out int offset)
    {
        int num = 0;
        int extraSpacing = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            OptionsButton optionsButton = buttons[i];
            optionsButton.TweenTo = new Vector2(200f, 45 + extraSpacing + i * 12);
            optionsButton.Position = optionsButton.TweenFrom = new Vector2((i % 2 == 0) ? (-160) : 480, 45 + extraSpacing + i * 12);

            if (optionsButton is not OptionsButtonHeader)
            {
                int i2 = 1;
                if (i > 0)
                {
                    var button = buttons[i - 1];
                    while (button is OptionsButtonHeader)
                    {
                        i2 += 1;
                        if (i - i2 < 0)
                        {
                            break;
                        }
                        button = buttons[i - i2];
                    }

                    optionsButton.UpItem = button;
                }
                i2 = 1;

                if (i < buttons.Count - 1)
                {
                    var button = buttons[i + i2];
                    while (button is OptionsButtonHeader)
                    {
                        i2 += 1;
                        if (i - i2 > buttons.Count)
                        {
                            break;
                        }
                        button = buttons[i + i2];
                        extraSpacing += 6;
                    }

                    optionsButton.DownItem = button;
                }
            }

            num += 9 + extraSpacing;
        }

        offset = num;
    }

    private class MusicOptionsButton : OptionsButton
    {
        public Action OnAlt;
        public MusicOptionsButton(string title) : base(title)
        {
        }

        public override void Update()
        {
            base.Update();

            if (MenuInput.Alt && Selected)
            {
                OnAlt?.Invoke();
            }
        }
    }
}