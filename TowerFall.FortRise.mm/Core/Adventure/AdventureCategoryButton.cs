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
        for (int i = 0; i < patch_GameData.AdventureWorldCategories.Count; i++) 
        {
            var item = patch_GameData.AdventureWorldCategories[i];

            int IamParticularlyHateCapturedValuesWithoutMyConsent = i;
            if (item == "::global::")
            {
                uiModal.AddItem("GLOBAL LEVELS", () => ChangeCategory(IamParticularlyHateCapturedValuesWithoutMyConsent));
                continue;
            }
            uiModal.AddItem(item, () => ChangeCategory(IamParticularlyHateCapturedValuesWithoutMyConsent));
        }

        uiModal.SetStartIndex(Map.CustomLevelCategory);
        Map.Add(uiModal);
    }

    private void ChangeCategory(int category) 
    {
        Map.CustomLevelCategory = category;
        Map.GotoAdventure();
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