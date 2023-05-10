using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_MainMenu : MainMenu
{
    private FortRise.FortModule currentModule;
    private float scrolling;
    private int totalScroll;
    private int count;
    private int scrollAmount = 12;
    private patch_MenuState state;
    private patch_MenuState switchTo;
    public patch_MenuState BackState;



    [MonoModIgnore]
    public patch_MenuState State
    {
        get
        {
            return state;
        }
        set
        {
            switchTo = value;
        }
    }

    [MonoModIgnore]
    public patch_MenuState OldState { get; set;  }

    [MonoModIgnore]
    public MenuItem ToStartSelected { get; private set; }


    public static patch_MatchSettings VersusMatchSettings;

    public patch_MainMenu(MenuState state) : base(state)
    {
    }

    public void CreateModOptions() 
    {
        var list = new List<OptionsButton>();
        currentModule.CreateSettings(list);
        if (list.Count > 0) 
        {
            InitOptions(list);
            ToStartSelected = list[0];
        }
        BackState = patch_MenuState.Mods;
        TweenUICameraToY(2);
    }

    public void DestroyModOptions() 
    {
        scrollAmount = 24;
        currentModule = null;
    }

    public void CreateMods() 
    {
        var list = new List<OptionsButton>();
        foreach (var mod in FortRise.RiseCore.InternalModules) 
        {
            var version = mod.MetaVersion.ToString();
            var setupName = mod.MetaName + " v" + version;
            var modButton = new OptionsButton(setupName.ToUpperInvariant() + "\n\n   " + mod.MetaAuthor.ToUpperInvariant());
            modButton.SetCallbacks(() => {
                State = patch_MenuState.ModOptions;
                currentModule = mod;
            });
            list.Add(modButton);
        }
        if (list.Count > 0) 
        {
            InitMods(list);
            ToStartSelected = list[0];
        }
        BackState = patch_MenuState.Main;
        TweenUICameraToY(1);

    }

    public void DestroyMods() 
    {
        if (switchTo != patch_MenuState.ModOptions)
        {
            SaveOnTransition = true;
        }
    }

    [MonoModIgnore]
    [PatchMainMenuCreateOptions]
    public extern void CreateOptions();

    private extern void orig_InitOptions(List<OptionsButton> buttons);

    private void InitOptions(List<OptionsButton> buttons) 
    {
        scrollAmount = 12;
        count = buttons.Count;
        orig_InitOptions(buttons);
    }

    private void InitMods(List<OptionsButton> buttons)
    {
        scrollAmount = 24;
        for (int i = 0; i < buttons.Count; i++)
        {
            var optionsButton = buttons[i];
            optionsButton.TweenTo = new Vector2(200f, (float)(45 + i * 24));
            optionsButton.Position = (
                optionsButton.TweenFrom = new Vector2((float)((i % 2 == 0) ? -160 : 480), (float)(45 + i * 24)));
            if (i > 0)
            {
                optionsButton.UpItem = buttons[i - 1];
            }
            if (i < buttons.Count - 1)
            {
                optionsButton.DownItem = buttons[i + 1];
            }
        }
        base.Add<OptionsButton>(buttons);
    }

    public extern void orig_Update();

    public override void Update()
    {
        if (state is patch_MenuState.Mods or patch_MenuState.ModOptions) 
        {
            if (MenuInput.Up && totalScroll > 0) 
            {
                scrolling += scrollAmount;
                totalScroll--;
            }
            if (MenuInput.Down && totalScroll < count) 
            {
                scrolling -= scrollAmount; 
                totalScroll++;
            }
            if (totalScroll > 9 && totalScroll < count - 5) 
            {
                foreach (var menuItem in Layers[-1].GetList<MenuItem>()) 
                {
                    menuItem.Position.Y += scrolling;
                }
            }
            scrolling = 0;
        }
        else 
        {
            scrolling = 0;
            totalScroll = 0;
        }
        orig_Update();
    }

    [MonoModIgnore]
    private extern void MainOptions();

    [MonoModIgnore]
    private extern void MainQuit();

    [MonoModIgnore]
    private extern void MainCredits();
    [MonoModIgnore]
    private extern void TweenBGCameraToY(int y);

    [MonoModReplace]
    public void CreateMain()
    {
        BladeButton quitButton = null;
        var list = new List<MenuItem>();

        FightButton fightButton = new FightButton(new Vector2(100f, 140f), new Vector2(-160f, 120f));
        list.Add(fightButton);

        CoOpButton coOpButton = new CoOpButton(new Vector2(220f, 140f), new Vector2(480f, 120f));
        list.Add(coOpButton);

        WorkshopButton workshopButton = new WorkshopButton(new Vector2(270f, 210f), new Vector2(270f, 300f));
        list.Add(workshopButton);

        ArchivesButton archivesButton = new ArchivesButton(new Vector2(200f, 210f), new Vector2(200f, 300f));
        list.Add(archivesButton);

        TrialsButton trialsButton = new TrialsButton(new Vector2(130f, 210f), new Vector2(130f, 300f));
        list.Add(trialsButton);

        BladeButton optionsButton;
        BladeButton modsButtons;
        BladeButton creditsButton;
        if (MainMenu.NoQuit)
        {
            modsButtons = new BladeButton(206 - 18f, "MODS", () => State = patch_MenuState.Mods);
            list.Add(modsButtons);
            optionsButton = new BladeButton(206f, "OPTIONS", this.MainOptions);
            list.Add(optionsButton);
            creditsButton = new BladeButton(224f, "CREDITS", this.MainCredits);
            list.Add(creditsButton);
        }
        else
        {
            modsButtons = new BladeButton(192f - 18f, "MODS", () => State = patch_MenuState.Mods);
            list.Add(modsButtons);
            optionsButton = new BladeButton(192f, "OPTIONS", this.MainOptions);
            list.Add(optionsButton);
            creditsButton = new BladeButton(210f, "CREDITS", this.MainCredits);
            list.Add(creditsButton);
            quitButton = new BladeButton(228f, "QUIT", this.MainQuit);
            list.Add(quitButton);
        }
        base.Add<MenuItem>(list);
        fightButton.DownItem = modsButtons;
        fightButton.RightItem = coOpButton;
        coOpButton.DownItem = trialsButton;
        coOpButton.LeftItem = fightButton;
        modsButtons.UpItem = fightButton;
        modsButtons.DownItem = optionsButton;
        modsButtons.RightItem = trialsButton;
        optionsButton.UpItem = modsButtons;
        optionsButton.DownItem = creditsButton;
        optionsButton.RightItem = trialsButton;
        creditsButton.UpItem = optionsButton;
        creditsButton.RightItem = trialsButton;
        if (!MainMenu.NoQuit)
        {
            creditsButton.DownItem = quitButton;
            quitButton.UpItem = creditsButton;
            quitButton.RightItem = trialsButton;
        }
        trialsButton.RightItem = archivesButton;
        trialsButton.LeftItem = optionsButton;
        trialsButton.UpItem = coOpButton;
        archivesButton.LeftItem = trialsButton;
        archivesButton.UpItem = coOpButton;
        if (workshopButton != null)
        {
            archivesButton.RightItem = workshopButton;
            coOpButton.DownItem = archivesButton;
            trialsButton.UpItem = fightButton;
            workshopButton.RightItem = null;
            workshopButton.LeftItem = archivesButton;
            workshopButton.UpItem = coOpButton;
        }
        ToStartSelected = OldState switch 
        {
            patch_MenuState.Options => optionsButton,
            patch_MenuState.Archives => archivesButton,
            patch_MenuState.Workshop => workshopButton,
            patch_MenuState.Credits => creditsButton,
            patch_MenuState.Mods => modsButtons,
            patch_MenuState.CoOp when RollcallMode is RollcallModes.Quest or RollcallModes.DarkWorld => coOpButton,
            _ when RollcallMode is RollcallModes.Trials => trialsButton,
            _ => fightButton

        };
        BackState = patch_MenuState.PressStart;
        TweenBGCameraToY(0);
        MainMenu.CurrentMatchSettings = null;
    }


    public enum patch_MenuState 
    {
        None,
        Loading,
        PressStart,
        Main,
        Fade,
        Rollcall,
        CoOp,
        VersusOptions,
        Variants,
        TeamSelect,
        Archives,
        Workshop,
        Options,
        Credits,
        ControlOptions,
        KeyboardConfig,
        Mods,
        ModOptions
    }
}