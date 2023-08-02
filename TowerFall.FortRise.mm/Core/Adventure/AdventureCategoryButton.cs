using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public sealed class AdventureCategoryButton : patch_MapButton
{
    public AdventureCategoryButton() : base("TOWER CATEGORY")
    {
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

        for (int i = 0; i < TowerRegistry.DarkWorldLevelSets.Count; i++) 
        {
            var item = TowerRegistry.DarkWorldLevelSets[i];
            uiModal.AddItem(item, () => ChangeLevelSet(item));
        }

        uiModal.SetStartIndex(Map.CustomLevelCategory);
        uiModal.OnBack = () => 
        {
            Map.Selection = this;
            Map.MapPaused = false;
        };
        Map.Add(uiModal);
    }

    private void ChangeLevelSet(string levelSet) 
    {
        if (levelSet == null)  
        {
            Map.ExitAdventure();
            Map.SetLevelSet("TowerFall");
            Map.MapPaused = false;
            return;
        }
        Map.SetLevelSet(levelSet);
        Map.GotoAdventure(Map.CurrentAdventureType);
        Map.MapPaused = false;
    }

    private void ChangeCategory(int category) 
    {
        Map.CustomLevelCategory = category;
        Map.GotoAdventure(Map.CurrentAdventureType);
        Map.MapPaused = false;
        var customMapRenderer = patch_GameData.AdventureWorldMapRenderer[Map.CustomLevelCategory];
        if (customMapRenderer.contains) 
        {
            if (Map.CurrentMapRender != null)
                Map.CurrentMapRender.Visible = false;
            Map.CurrentMapRender = customMapRenderer.renderer;
            Map.Renderer.Visible = false;
            Map.CurrentMapRender.Visible = true;

            if (Map.Selection == null || Map.Selection.Data == null) 
            {
                Map.CurrentMapRender.OnSelectionChange("");
                return;
            }
            Map.CurrentMapRender.OnSelectionChange(Map.Selection.Data.Title);
        }
        else 
        {
            if (Map.CurrentMapRender != null)
                Map.CurrentMapRender.Visible = false;
            Map.Renderer.Visible = true;
            Map.CurrentMapRender = null;

            if (Map.Selection == null || Map.Selection.Data == null)
            {
                Map.Renderer.OnSelectionChange("");
                return;
            }
            Map.Renderer.OnSelectionChange(Map.Selection.Data.Title);
        }
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