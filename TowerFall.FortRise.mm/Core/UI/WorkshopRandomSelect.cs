using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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

    // Token: 0x060022E3 RID: 8931 RVA: 0x0010C860 File Offset: 0x0010AA60
    public override void OnConfirm()
    {
        Music.Stop();

        MapButton randomVersusTower = (Map as patch_MapScene).GetRandomWorkshopTower();

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

