#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MapButton : MapButton
{
    [MonoModIgnore]
    public patch_MapScene Map { get; set; } 



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
        TowerTheme theme = patch_GameData.AdventureWorldTowers[levelID].Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        list.Add(image2);
        image2.Color = MapButton.GetTint(theme.TowerType);

        return list;
    }

}