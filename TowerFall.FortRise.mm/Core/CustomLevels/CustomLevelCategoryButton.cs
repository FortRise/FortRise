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

        var textContainer = new TextContainer(160);
        textContainer.LayerIndex = 0;
        textContainer.WithFade = true;
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

        List<string> sets = Mode switch
        {
            MainMenu.RollcallModes.Versus => TowerRegistry.VersusLevelSets,
            MainMenu.RollcallModes.Quest => TowerRegistry.QuestLevelSets,
            MainMenu.RollcallModes.DarkWorld => TowerRegistry.DarkWorldLevelSets,
            MainMenu.RollcallModes.Trials => TowerRegistry.TrialsLevelSet,
            _ => throw new NotImplementedException()
        };

        int startIndex = 0;
        for (int i = 0; i < sets.Count; i++) 
        {
            var item = sets[i];
            if (Map.GetLevelSet() == item)
            {
                startIndex = i + 2;
            }
            var modButton = new BowButton(item);
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