using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public sealed class AdventureCategoryButton : patch_MapButton
{
    public AdventureType Type;
    public AdventureCategoryButton(AdventureType type) : base("TOWER CATEGORY")
    {
        Type = type;
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
        var towerFallButton = new BowButton("TowerFall");
        towerFallButton.Pressed(() => {
            ChangeLevelSet(null);
            textContainer.RemoveSelf();
        });
        textContainer.Add(towerFallButton);

        List<string> sets = null;
        switch (Type) 
        {
        case AdventureType.Quest:
            sets = TowerRegistry.QuestLevelSets;
            break;
        case AdventureType.DarkWorld:
            sets = TowerRegistry.DarkWorldLevelSets;
            break;
        case AdventureType.Versus:
            sets = TowerRegistry.VersusLevelSets;
            break;
        case AdventureType.Trials:
            sets = TowerRegistry.QuestLevelSets;
            break;
        }

        int startIndex = 0;
        for (int i = 0; i < sets.Count; i++) 
        {
            var item = sets[i];
            if (Map.GetLevelSet() == item)
                startIndex = i + 1;
            var modButton = new BowButton(UncategorizedIfGlobal(item));
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

        string UncategorizedIfGlobal(string item) 
        {
            if (item == "::global::") 
            {
                return "Uncategorized";
            }
            return item;
        }
    }

    private void ChangeLevelSet(string levelSet) 
    {
        if (levelSet == null)  
        {
            Map.ExitAdventure();
            Map.MapPaused = false;
            return;
        }

        Map.Renderer.ChangeLevelSet(levelSet);
        Map.SetLevelSet(levelSet);
        Map.GotoAdventure(Map.CurrentAdventureType);
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