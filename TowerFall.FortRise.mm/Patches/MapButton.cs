using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MapButton : MapButton
{
    [MonoModIgnore]
    public patch_MapScene Map { get; set; } 
    public string Author { get; set; }



    public patch_MapButton(string title) : base(title)
    {
    }


    [MonoModIgnore]
    public extern override void OnConfirm();

    [MonoModIgnore]
    protected extern override List<Image> InitImages();

    public extern static Image[] orig_InitDarkWorldStartLevelGraphics(int levelID);

    public static Image[] InitDarkWorldStartLevelGraphics(int levelID)
    {
        if (!patch_SaveData.AdventureActive) 
        {
            return orig_InitDarkWorldStartLevelGraphics(levelID);
        }
        TowerTheme theme = patch_GameData.AdventureWorldTowers[levelID].Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        image2.Color = MapButton.GetTint(theme.TowerType);
        list.Add(image2);
        return list.ToArray();
    }

    public static List<Image> InitAdventureWorldGraphics(int levelID)
    {
        AdventureWorldData worldData = patch_GameData.AdventureWorldTowers[levelID];
        AdventureWorldTowerStats stats = worldData.Stats;
        TowerTheme theme = worldData.Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        list.Add(image2);
        image2.Color = MapButton.GetTint(theme.TowerType);

        if (stats.EarnedGoldEye)
        {
            Image image3 = new Image(TFGame.MenuAtlas["questResults/goldEye"], null);
            image3.CenterOrigin();
            image3.Origin += new Vector2(10f, 10f);
            list.Add(image3);
        }
        else if (stats.EarnedEye)
        {
            Image image4 = new Image(TFGame.MenuAtlas["questResults/eye"], null);
            image4.CenterOrigin();
            image4.Origin += new Vector2(10f, 10f);
            list.Add(image4);
        }
        else if (stats.CompletedLegendary)
        {
            Image image5 = new Image(TFGame.MenuAtlas["questResults/goldSkull"], null);
            image5.CenterOrigin();
            image5.Origin += new Vector2(10f, 10f);
            list.Add(image5);
        }
        else if (stats.CompletedHardcore)
        {
            Image image6 = new Image(TFGame.MenuAtlas["questResults/redSkull"], null);
            image6.CenterOrigin();
            image6.Origin += new Vector2(10f, 10f);
            list.Add(image6);
        }
        else if (stats.CompletedNormal)
        {
            Image image7 = new Image(TFGame.MenuAtlas["questResults/whiteSkull"], null);
            image7.CenterOrigin();
            image7.Origin += new Vector2(10f, 10f);
            list.Add(image7);
        }

        return list;
    }

}