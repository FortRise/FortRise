#pragma warning disable CS0626
#pragma warning disable CS0108

using System.Collections.Generic;
using MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using FortRise;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace TowerFall;

public class patch_MainMenu : MainMenu
{
    public static patch_RollcallModes RollcallMode;
    private patch_MenuState state;
    private patch_MenuState switchTo;
    public patch_MenuState State 
    {
        get => state;
        set => switchTo = value;
    }
    public patch_MainMenu(MenuState state) : base(state) {}

    [MonoModIgnore]
    public MenuItem ToStartSelected { get; set; }

    [MonoModIgnore]
    private extern void TweenBGCameraToY(int y);
    [MonoModIgnore]
    private extern void MainOptions();
    [MonoModIgnore]
    private extern void MainCredits();
    [MonoModIgnore]
    private extern void MainQuit();
    
    public void CreateMain() 
    {
        BladeButton bladeButton = null;
        List<MenuItem> list = new List<MenuItem>();
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
        BladeButton bladeButton2;
        BladeButton bladeButton3;
        if (MainMenu.NoQuit)
        {
            bladeButton2 = new BladeButton(206f, "OPTIONS", new Action(this.MainOptions));
            list.Add(bladeButton2);
            bladeButton3 = new BladeButton(224f, "CREDITS", new Action(this.MainCredits));
            list.Add(bladeButton3);
        }
        else
        {
            bladeButton2 = new BladeButton(192f, "OPTIONS", new Action(this.MainOptions));
            list.Add(bladeButton2);
            bladeButton3 = new BladeButton(210f, "CREDITS", new Action(this.MainCredits));
            list.Add(bladeButton3);
            bladeButton = new BladeButton(228f, "QUIT", new Action(this.MainQuit));
            list.Add(bladeButton);
        }

        fightButton.DownItem = bladeButton2;
        fightButton.RightItem = coOpButton;
        coOpButton.DownItem = trialsButton;
        coOpButton.LeftItem = fightButton;
        bladeButton2.UpItem = fightButton;
        bladeButton2.DownItem = bladeButton3;
        bladeButton2.RightItem = trialsButton;
        bladeButton3.UpItem = bladeButton2;
        bladeButton3.RightItem = trialsButton;
        if (!MainMenu.NoQuit)
        {
            bladeButton3.DownItem = bladeButton;
            bladeButton.UpItem = bladeButton3;
            bladeButton.RightItem = trialsButton;
        }
        trialsButton.RightItem = archivesButton;
        trialsButton.LeftItem = bladeButton2;
        trialsButton.UpItem = coOpButton;
        archivesButton.LeftItem = trialsButton;
        archivesButton.UpItem = coOpButton;
        AddModMenu(fightButton, bladeButton2, list);
        base.Add<MenuItem>(list);
        if (workshopButton != null)
        {
            archivesButton.RightItem = workshopButton;
            coOpButton.DownItem = archivesButton;
            trialsButton.UpItem = fightButton;
            workshopButton.RightItem = null;
            workshopButton.LeftItem = archivesButton;
            workshopButton.UpItem = coOpButton;
        }
        if (this.OldState == MainMenu.MenuState.Options)
        {
            this.ToStartSelected = bladeButton2;
        }
        else if (this.OldState == MainMenu.MenuState.Archives)
        {
            this.ToStartSelected = archivesButton;
        }
        else if (this.OldState == MainMenu.MenuState.Workshop)
        {
            this.ToStartSelected = workshopButton;
        }
        else if (this.OldState == MainMenu.MenuState.Credits)
        {
            this.ToStartSelected = bladeButton3;
        }
        else if (this.OldState == MainMenu.MenuState.CoOp || MainMenu.RollcallMode == MainMenu.RollcallModes.Quest || MainMenu.RollcallMode == MainMenu.RollcallModes.DarkWorld)
        {
            this.ToStartSelected = coOpButton;
        }
        else if (MainMenu.RollcallMode == MainMenu.RollcallModes.Trials)
        {
            this.ToStartSelected = trialsButton;
        }
        else
        {
            this.ToStartSelected = fightButton;
        }
        this.BackState = MainMenu.MenuState.PressStart;
        this.TweenBGCameraToY(0);
        MainMenu.CurrentMatchSettings = null;
    }

    private void AddModMenu(FightButton fight, BladeButton down, List<MenuItem> list) 
    {
        var bladeButton = new BladeButton(192 - 18, "MODS", this.MainMods);
        list.Add(bladeButton);
        bladeButton.DownItem = down;
        bladeButton.UpItem = fight;
        fight.DownItem = bladeButton;
        down.UpItem = bladeButton;
    }

    private void MainMods() 
    {
        State = patch_MenuState.Mods;
    }

    public void CreateMods() 
    {
        
    }
    
    public void DestroyMods() 
    {

    }

    public void CreateCoOp() 
    {
        var list = new List<MenuItem>();
        QuestButton questButton = new QuestButton(new Vector2(100f, 90f), new Vector2(-160f, 120f));
        list.Add(questButton);

        DarkWorldButton darkWorldButton = new DarkWorldButton(new Vector2(220f, 90f), new Vector2(480f, 120f));
        list.Add(darkWorldButton);

        // AdventureButton adventureButton = new AdventureButton(new Vector2(300, 90f), new Vector2(500, 120f));
        // list.Add(adventureButton);
        
        base.Add<MenuItem>(list);
        questButton.RightItem = darkWorldButton;
        darkWorldButton.LeftItem = questButton;
        // darkWorldButton.RightItem = adventureButton;
        // adventureButton.LeftItem = darkWorldButton;
        ToStartSelected = RollcallMode switch 
        {
            // patch_RollcallModes.Adventure => adventureButton,
            patch_RollcallModes.DarkWorld => darkWorldButton,
            _ => questButton
        };

        this.BackState = MainMenu.MenuState.Main;
        this.TweenBGCameraToY(1);
        base.Add<CoOpDataDisplay>(new CoOpDataDisplay(questButton, darkWorldButton));
    }

    private bool Instruction(Instruction instr) 
    {
        return instr.MatchLdcI4(1);
    } 

    [PatchCreateRollcall]
    [MonoModIgnore]
    public extern void CreateRollcall();

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
        Mods
    }

    public enum patch_RollcallModes 
    {
        Versus,
        Quest,
        DarkWorld,
        Trials,
        Adventure
    }
}