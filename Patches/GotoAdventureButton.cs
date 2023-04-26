#pragma warning disable CS0626
#pragma warning disable CS0108

using System.Collections.Generic;
using Monocle;
using MonoMod;

namespace TowerFall;

public sealed class GotoAdventureButton : patch_MapButton
{
    [MonoModIgnore]
    public TowerMapData Data { get; private set; }


    public GotoAdventureButton() : base("ADVENTURE LEVELS")
    {
    }

    public override void OnConfirm()
    {
        Map.Selection = null;
        OnDeselect();
        Map.GotoAdventure();
        Map.MatchStarting = false;
    }

    protected override List<Image> InitImages()
    {
        return MapButton.InitWorkshopGraphics();
    }

}
