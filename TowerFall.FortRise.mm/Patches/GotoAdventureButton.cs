using System.Collections.Generic;
using FortRise.Adventure;
using Monocle;

namespace TowerFall;

public sealed class GotoAdventureButton : patch_MapButton
{
    public AdventureType Type;
    public GotoAdventureButton(AdventureType type) : base("ADVENTURE LEVELS")
    {
        Type = type;
    }

    public override void OnConfirm()
    {
        Map.Selection = null;
        OnDeselect();
        Map.GotoAdventure(Type);
        Map.MatchStarting = false;
    }

    protected override List<Image> InitImages()
    {
        return MapButton.InitWorkshopGraphics();
    }

}
