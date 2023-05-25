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
    public float TweenAt { get; private set; }
    


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
        AdventureWorldTowerData worldData = patch_GameData.AdventureWorldTowers[levelID];
        AdventureWorldTowerStats stats = worldData.Stats;
        TowerTheme theme = worldData.Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType));
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon);
        image2.CenterOrigin();
        list.Add(image2);
        image2.Color = MapButton.GetTint(theme.TowerType);

        var path = stats switch 
        {
            { EarnedGoldEye: true } => "questResults/goldEye",
            { EarnedEye: true } => "questResults/eye",
            { CompletedLegendary: true } => "questResults/goldSkull",
            { CompletedHardcore: true } => "questResults/redSkull",
            { CompletedNormal: true } => "questResults/whiteSkull",
            _ => string.Empty
        };

        if (path != string.Empty) 
        {
            var skull = new Image(TFGame.MenuAtlas[path]) ;
            skull.CenterOrigin();
            skull.Origin += new Vector2(10f, 10f);
            list.Add(skull);
        }

        return list;
    }

    // This function fixed the memory leak
    // It's basically destroying the entity when the animation is complete
    // It is good to do this as it will marked by GC as this button is no longer use anymore and should be disposed
    public void TweenOutAndRemoved()
    {
        float startY = Y;
        float startTween = TweenAt;
        var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 20, true);
        tween.OnUpdate = t =>
        {
            Y = MathHelper.Lerp(startY, 300f + this.AddY, t.Eased);
            TweenAt = MathHelper.Lerp(startTween, 0f, t.Eased);
        };
        tween.OnComplete = t => 
        {
            RemoveSelf();
        };
        Add(tween);
    }
}