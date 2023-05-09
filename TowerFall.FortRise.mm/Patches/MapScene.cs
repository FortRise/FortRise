using System.Collections;
using System.Collections.Generic;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MapScene : MapScene
{
    private bool adventureLevels;
    public patch_MapScene(MainMenu.RollcallModes mode) : base(mode)
    {
    }

    [MonoModIgnore]
    [PatchMapSceneBegin]
    public extern override void Begin();

    [MonoModReplace]
    private IEnumerator DarkWorldIntroSequence() 
    {
        int num = 0;
        for (int i = 0; i < Buttons.Count; i = num + 1) 
        {
            if (Buttons[i] is not DarkWorldMapButton)
                continue;
            if (SaveData.Instance.DarkWorld.ShouldRevealTower(Buttons[i].Data.ID.X)) 
            {
                Music.Stop();
                yield return Buttons[i].UnlockSequence(false);
            }
            num = i;
        }
        yield break;
    }
    

    private void InitAdventureMap() 
    {
        Buttons.Add(new GotoAdventureButton());
    }

    public void InitAdventure() 
    {
        Add(new AdventureListLoader(this));
    }

    public void GotoAdventure() 
    {
        patch_SaveData.AdventureActive = true;
        adventureLevels = true;
        WorkshopLevels = true;
        TweenOutAllButtons();
        Buttons.Clear();
        InitAdventure();
    }

    public extern void orig_ExitWorkshop();

    public void ExitWorkshop() 
    {
        if (adventureLevels)
            ExitAdventure();
        else
            orig_ExitWorkshop();
    }

    public void ExitAdventure() 
    {
        patch_SaveData.AdventureActive = false;
        adventureLevels = false;
        WorkshopLevels = false;
        TweenOutAllButtons();
        Buttons.Clear();
        Buttons.Add(new GotoAdventureButton());
        for (int j = 0; j < GameData.DarkWorldTowers.Count; j++)
        {
            if (SaveData.Instance.Unlocks.GetDarkWorldTowerUnlocked(j))
            {
                Buttons.Add(new DarkWorldMapButton(GameData.DarkWorldTowers[j]));
            }
        }
        this.LinkButtonsList();
        InitButtons(Buttons[1]);
        foreach (var button in Buttons)
            Add(button);
        ScrollToButton(Selection);
    }

    private extern void orig_Update();

    public override void Update()
    {
        if (!MatchStarting && Mode == MainMenu.RollcallModes.DarkWorld) 
        {
            if (MenuInput.Up && !patch_SaveData.AdventureActive) 
            {
                GotoAdventure();
            }
            else if (MenuInput.Down && patch_SaveData.AdventureActive) 
            {
                ExitAdventure();
            }
        }
        orig_Update();
    }

    // Fixing the memory leak that TowerFall has
    [MonoModReplace]
    public void TweenOutAllButtons() 
    {
        foreach (var mapButton in Buttons) 
        {
            (mapButton as patch_MapButton).TweenOutAndRemoved();
        }
    }
}