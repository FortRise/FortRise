using System.Collections.Generic;
using Monocle;
using MonoMod;

namespace TowerFall;

public sealed class AdventureMapButton : MapButton
{
    // Quite needed to do this to access the private set
    [MonoModIgnore]
    public new string Author { get; set; }
    private string author;


    public AdventureMapButton(AdventureWorldData data) : base(new TowerMapData(data))
    {
        author = data.Author.ToUpperInvariant();
    }
    public override void Added()
    {
        Author = author;
        base.Added();
    }

    protected override bool GetLocked()
    {
        return false;
    }

    public override void OnConfirm()
    {
        MainMenu.DarkWorldMatchSettings.LevelSystem = base.Data.GetLevelSystem();
        base.Map.TweenOutButtons();
        base.Map.Add<DarkWorldDifficultySelect>(new DarkWorldDifficultySelect());
    }

    protected override List<Image> InitImages()
    {
        return patch_MapButton.InitAdventureWorldGraphics(Data.ID.X);
    }
}