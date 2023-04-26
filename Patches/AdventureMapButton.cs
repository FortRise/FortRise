using System.Collections.Generic;
using Monocle;

namespace TowerFall;

public sealed class AdventureMapButton : MapButton
{
    public AdventureMapButton(AdventureWorldData data) : base(new TowerMapData(data))
    {
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