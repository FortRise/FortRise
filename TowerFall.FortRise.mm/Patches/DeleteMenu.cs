namespace TowerFall;

public sealed class DeleteMenu : UIModal
{
    private patch_MapScene map;

    public DeleteMenu(patch_MapScene map, int id) : base(0)
    {
        this.map = map;
        Title = "DELETE LEVEL";
        SelectionFlash = true;
        HideTitle(true);

        AddFiller("Delete the level");
        AddItem("YES", () => {
            var level = patch_GameData.AdventureWorldTowers[id];
            patch_GameData.AdventureWorldTowers.Remove(level);
            patch_GameData.AdventureWorldTowersLoaded.Remove(level.StoredDirectory);
            patch_SaveData.AdventureActive = false;
            UploadMapButton.SaveLoaded();
            patch_GameData.ReloadCustomLevels();
            map.GotoAdventure();
            RemoveSelf();
        });
        AddItem("NO", () => {
            RemoveSelf();
            Visible = false;
            map.MapPaused = false;
        });

        OnBack = () => map.MapPaused = false;
    }

    public override void Removed()
    {
        base.Removed();
        map.MapPaused = false;
    }
}