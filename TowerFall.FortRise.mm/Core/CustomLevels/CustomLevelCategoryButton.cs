using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

public sealed class CustomLevelCategoryButton : patch_MapButton
{
    public MainMenu.RollcallModes Mode;
    public CustomLevelCategoryButton(MainMenu.RollcallModes mode) : base("TOWER CATEGORY")
    {
        Mode = mode;
    }

    public override void OnConfirm()
    {
        Map.Selection = null;
        OnDeselect();
        Map.MatchStarting = false;
        Map.MapPaused = true;

        var textContainer = new TextContainer(160)
        {
            LayerIndex = 0,
            WithFade = true
        };

        textContainer.BackAction = () => 
        {
            Map.Selection = this;
            Map.MapPaused = false;
            Sounds.ui_unpause.Play(160f);
            textContainer.RemoveSelf();
        };
        textContainer.Add(new TextContainer.HeaderText("Select Category"));
        var towerFallButton = new BowButton("TowerFall");
        towerFallButton.Pressed(() => {
            ChangeLevelSet(null);
            textContainer.RemoveSelf();
        });
        textContainer.Add(towerFallButton);

        var sets = CreateLevelSets();
        ModEventsManager.Instance.OnLevelSetsCreated.Raise(this, new(Map, Mode, sets));

        int startIndex = 0;
        for (int i = 0; i < sets.Count; i++) 
        {
            var item = sets[i];
            if (Map.GetLevelSet() == item)
            {
                startIndex = i + 2;
            }

            string displaySet = item;
            int last = item.LastIndexOf('/');
            if (last != -1)
            {
                displaySet = item[(last + 1)..];
            }

            var modButton = new BowButton(displaySet);
            modButton.Pressed(() => {
                ChangeLevelSet(item);
                textContainer.RemoveSelf();
            });
            textContainer.Add(modButton);
        }

        Map.Add(textContainer);
        textContainer.Selection = startIndex;
        textContainer.Selected = true;
        textContainer.TweenIn();
    }

    private List<string> CreateLevelSets()
    {
        List<string> sets = Mode switch
        {
            MainMenu.RollcallModes.Versus => TowerRegistry.VersusLevelSets,
            MainMenu.RollcallModes.Quest => TowerRegistry.QuestLevelSets,
            MainMenu.RollcallModes.DarkWorld => TowerRegistry.DarkWorldLevelSets,
            MainMenu.RollcallModes.Trials => TowerRegistry.TrialsLevelSet,
            _ => throw new NotImplementedException()
        };

        // clone the list, so the original will not be modified
        return [..sets];
    }

    private void ChangeLevelSet(string levelSet) 
    {
        if (levelSet == null)  
        {
            Map.ExitCustomLevels();
            Map.MapPaused = false;
            return;
        }

        Map.Renderer.ChangeLevelSet(levelSet);
        Map.SetLevelSet(levelSet);
        Map.ChangeLevelSet();
        Map.MapPaused = false;
    }

    protected override List<Image> InitImages()
    {
        Image image = new Image(TFGame.MenuAtlas["randomLevelBlock"], null);
        image.CenterOrigin();
        Image image2 = new Image(patch_TFGame.FortRiseMenuAtlas["icon/levelcategoryicon"], null);
        image2.CenterOrigin();
        return new List<Image> { image, image2 };
    }
}
