using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

public class WorkshopRandomSelect : MapButton
{
    public WorkshopRandomSelect()
        : base("RANDOM")
    {
    }

    protected override List<Image> InitImages() => InitRandomVersusGraphics();

    public override void OnConfirm()
    {
        Music.Stop();

        MapButton randomVersusTower = (Map as TowerFall.Patching.MapScene).GetRandomWorkshopTower();

        MainMenu.VersusMatchSettings.LevelSystem = randomVersusTower.Data.GetLevelSystem();
        MainMenu.VersusMatchSettings.RandomVersusTower = true;

        Map.ScrollToButton(randomVersusTower);
        MapButton mapButton = Map.Selection;

        int iteration = 0;
        while (mapButton != randomVersusTower)
        {
            mapButton = mapButton.RightButton;
            MapButton current = mapButton;
            Alarm.Set(this, iteration * 6 + 1, () => Map.SelectLevel(current));
            iteration++;
        }

        Alarm.Set(this, iteration * 6 + 30, () =>
        {
            Map.TweenOutButtons();
            Map.Selection.OnConfirm();
        });
    }


    public override bool HasAltAction
    {
        get
        {
            return true;
        }
    }

    public override void AltAction()
    {
        Map.ClearNoRandoms();
    }
}

