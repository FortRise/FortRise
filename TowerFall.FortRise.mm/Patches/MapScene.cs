using System.Collections;
using System.Collections.Generic;
using FortRise.Adventure;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MapScene : MapScene
{
    private static int lastRandomVersusTower;
    private bool adventureLevels;
    private float crashDelay;
    public bool MapPaused;
    public int CustomLevelCategory;
    public patch_MapScene(MainMenu.RollcallModes mode) : base(mode)
    {
    }

    private void ExtendCtor() 
    {
        CustomLevelCategory = -1;
        crashDelay = 10;
    }

    [MonoModConstructor]
    [MonoModReplace]
    public static void cctor() {}

    internal static void FixedStatic() 
    {
        lastRandomVersusTower = -1;
        MapScene.NoRandomStates = new bool[GameData.VersusTowers.Count];
    }

    [MonoModIgnore]
    [PatchMapSceneBegin]
    [PreFixing("TowerFall.MapScene", "System.Void ExtendCtor()")]
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

    private void InitAdventureMap(List<MapButton[]> list) 
    {
        var gotoAdventure = new GotoAdventureButton();
        Buttons.Add(gotoAdventure);
        list.Add(new MapButton[] { gotoAdventure, gotoAdventure, gotoAdventure });
    }

    public void InitAdventure(int id) 
    {
        Add(new AdventureListLoader(this, id));
    }

    public void GotoAdventure(int id = 0) 
    {
        patch_SaveData.AdventureActive = true;
        adventureLevels = true;
        WorkshopLevels = true;
        TweenOutAllButtonsAndRemove();
        Buttons.Clear();
        patch_GameData.AdventureWorldTowers = patch_GameData.AdventureWorldModTowers[CustomLevelCategory];
        InitAdventure(id);
    }

    public extern void orig_ExitWorkshop();

    public void ExitWorkshop() 
    {
        if (adventureLevels)
            ExitAdventure();
        else
            orig_ExitWorkshop();
    }

    public void ExitAdventure(int id = 1) 
    {
        patch_SaveData.AdventureActive = false;
        adventureLevels = false;
        WorkshopLevels = false;
        TweenOutAllButtonsAndRemove();
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
        if (id >= Buttons.Count)
            id = Buttons.Count - 1;
        InitButtons(Buttons[id]);
        foreach (var button in Buttons)
            Add(button);
        ScrollToButton(Selection);
    }

    [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
    public void base_Update() 
    {
        base.Update();
    }

    private extern void orig_Update();

    public override void Update()
    {
        if (MapPaused) 
        {
            base_Update();
            return;
        }
        if (!ScrollMode && !MatchStarting && Mode == MainMenu.RollcallModes.DarkWorld && crashDelay <= 0) 
        {
            if (MenuInput.Alt2 && Selection is AdventureMapButton button)
            {
                var id = Selection.Data.ID.X;
                var level = patch_GameData.AdventureWorldTowers[id];
                if (AdventureModule.SaveData.LevelLocations.Contains(level.StoredDirectory)) 
                {
                    Add(new DeleteMenu(this, id));
                    MapPaused = true;
                    return;
                }
                button.Shake();
                MenuInput.RumblePlayers(1f, 20);
            }
            if (MenuInput.Up) 
            {
                if (CustomLevelCategory != patch_GameData.AdventureWorldModTowers.Count - 1) 
                {
                    CustomLevelCategory++;
                    var id = Buttons.IndexOf(Selection);
                    GotoAdventure(id);
                    if (Selection.Data == null)
                    {
                        Renderer.OnSelectionChange("");
                        return;
                    }
                    Renderer.OnSelectionChange(Selection.Data.Title);
                }
            }
            else if (MenuInput.Down && patch_SaveData.AdventureActive) 
            {
                if (CustomLevelCategory != -1)
                    CustomLevelCategory--;

                var id = Buttons.IndexOf(Selection);
                if (CustomLevelCategory == -1) 
                {
                    ExitAdventure(id);
                    if (Selection.Data == null)
                    {
                        Renderer.OnSelectionChange("");
                        return;
                    }
                }
                else 
                {
                    GotoAdventure(id);
                    if (Selection.Data == null)
                    {
                        Renderer.OnSelectionChange("");
                        return;
                    }
                }
                Renderer.OnSelectionChange(Selection.Data.Title);
            }
            if (MenuInput.Back) 
            {
                CustomLevelCategory = -1;
            }

            if (MInput.Keyboard.Pressed(Keys.F5)) 
            {
                var id = Buttons.IndexOf(Selection);
                patch_GameData.ReloadCustomLevels();
                GotoAdventure(id);
            }
        }
        orig_Update();
        if (crashDelay > 0)
            crashDelay--;
    }

    public void TweenOutAllButtonsAndRemove() 
    {
        foreach (var mapButton in Buttons) 
        {
            (mapButton as patch_MapButton).TweenOutAndRemoved();
        }
    }
}