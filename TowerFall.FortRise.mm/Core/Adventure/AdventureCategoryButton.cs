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

        var uiModal = new UIModal(0);
        uiModal.SetTitle("SELECT CATEGORY");
        uiModal.AutoClose = true;
        uiModal.AddItem("TowerFall", () => ChangeLevelSet(null));

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
        for (int i = 0; i < sets.Count; i++) 
        {
            var item = sets[i];
            uiModal.AddItem(UncategorizedIfGlobal(item), () => ChangeLevelSet(item));
        }

        uiModal.SetStartIndex(Map.GetLevelSet());
        uiModal.OnBack = () => 
        {
            Map.Selection = this;
            Map.MapPaused = false;
        };
        Map.Add(uiModal);

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