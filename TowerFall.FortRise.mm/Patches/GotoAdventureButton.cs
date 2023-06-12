using System.Collections.Generic;
using Monocle;

namespace TowerFall;

public sealed class GotoAdventureButton : patch_MapButton
{
    public GotoAdventureButton() : base("ADVENTURE LEVELS")
    {
    }

    public override void OnConfirm()
    {
        Map.Selection = null;
        OnDeselect();
        Map.CustomLevelCategory++;
        Map.GotoAdventure();
        Map.MatchStarting = false;
    }

    protected override List<Image> InitImages()
    {
        return MapButton.InitWorkshopGraphics();
    }

}
