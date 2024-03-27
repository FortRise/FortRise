using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.Adventure;

public class AdventureChaoticRandomSelect : VersusRandomSelect 
{
    public AdventureChaoticRandomSelect() : base() 
    {
        this.SetTitle("CHAOTIC RANDOM");
    }

    public override void OnConfirm()
    {
        (Map as patch_MapScene).TweenOutAllButtonsAndRemoveExcept(this);
        this.TweenOut();
        Map.Buttons.Clear();
        var buttons = new List<MapButton>();

        foreach (var levelSet in TowerRegistry.VersusTowerSets) 
        {
            Map.SetLevelSet(levelSet.Key);
            foreach (var versusLevels in levelSet.Value) 
            {
                var adv = new AdventureMapButton(versusLevels, levelSet.Key, AdventureType.Versus);
                if (adv.Locked) 
                    continue;
                buttons.Add(adv);
                Map.Add(adv);
            }
        }
        Map.SetLevelSet("TowerFall");

        for (int i = 0; i < GameData.VersusTowers.Count; i++)
        {
            if (SaveData.Instance.Unlocks.GetTowerUnlocked(i))
            {
                var button = new VersusMapButton(GameData.VersusTowers[i]);
                buttons.Add(button);
                Map.Add(button);
            }
        }
        buttons.Shuffle();
        Map.Buttons = buttons;
        Map.LinkButtonsList();
        Map.InitButtons(buttons[0]);
        Map.ScrollToButton(Map.Selection);

        Alarm.Set(this, 10, () => 
        {
            Music.Stop();
            MapButton randomVersusTower = Map.GetRandomVersusTower();
            if (randomVersusTower is AdventureMapButton button) 
            {
                Map.SetLevelSet(button.LevelSet);
            }

			MainMenu.VersusMatchSettings.LevelSystem = randomVersusTower.Data.GetLevelSystem();
			MainMenu.VersusMatchSettings.RandomVersusTower = true;
			if (MainMenu.VersusMatchSettings.LevelSystem.Procedural)
			{
				Map.SetSeed(false);
			}
			Map.ScrollToButton(randomVersusTower);
			MapButton mapButton = (base.Scene as MapScene).Selection;
			int num = 0;
			while (mapButton != randomVersusTower)
			{
				mapButton = mapButton.RightButton;
				MapButton current = mapButton;
				Alarm.Set(this, num * 6 + 1, () =>
				{
					Map.SelectLevel(current, false);
				});
				num++;
			}
			Alarm.Set(this, num * 6 + 30, () =>
			{
				Map.TweenOutButtons();
				Map.DoEnterAreaZoom(MainMenu.VersusMatchSettings.LevelSystem.Theme.MapPosition);
			});
        });
    }


    protected override List<Image> InitImages()
    {
        Image image = new Image(TFGame.MenuAtlas["randomLevelBlock"], null);
        image.CenterOrigin();
        Image image2 = new Image(patch_TFGame.FortRiseMenuAtlas["icon/randomizedSets"], null);
        image2.CenterOrigin();
        image2.Color = Color.Gray;
        return new List<Image> { image, image2 };
    }
}