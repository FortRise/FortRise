using System.Collections.Generic;
using FortRise.Adventure;
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


    public string Title { [MonoModIgnore] get => null; [MonoModIgnore] [MonoModPublic] set => value = null; }
    


    public patch_MapButton(string title) : base(title)
    {
    }


    [MonoModIgnore]
    public extern override void OnConfirm();

    [MonoModIgnore]
    protected extern override List<Image> InitImages();

    public static Image[] InitDarkWorldStartLevelGraphics(int levelID, string levelSet)
    {
        if (levelSet == "TowerFall") 
        {
            return InitDarkWorldStartLevelGraphics(levelID);
        }
        TowerTheme theme = TowerRegistry.DarkWorldTowerSets[levelSet][levelID].Theme;
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

    public static List<Image> InitAdventureMapButtonGraphics(Point levelID) 
    {
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
            return new List<Image>();
        switch (scene.GetCurrentAdventureType()) 
        {
        case AdventureType.DarkWorld:
            return InitAdventureWorldGraphics(levelID.X);
        case AdventureType.Quest:
            return InitAdventureQuestGraphics(levelID.X);
        case AdventureType.Versus:
            return InitAdventureVersusGraphics(levelID.X);
        case AdventureType.Trials:
            return InitAdventureTrialsGraphics(levelID); 
        default:
            return new List<Image>();
        }
    }

    public static List<Image> InitAdventureTrialsGraphics(Point levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
            return new List<Image>();
        var tower = (AdventureTrialsTowerData)TowerRegistry.TrialsTowerSets[scene.GetLevelSet()][levelID.X][levelID.Y];
        var theme = tower.Theme;
        var list = new List<Image>();
        if (levelID.Y == 0)
        {
            Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
            image.CenterOrigin();
            list.Add(image);
            Image image2 = new Image(theme.Icon, null);
            image2.CenterOrigin();
            image2.Color = MapButton.GetTint(theme.TowerType);
            list.Add(image2);
            if (levelID.Y > 0)
            {
                Image image3 = new Image(MapButton.GetNumeralTexture(theme.TowerType, levelID.Y), null);
                image3.Origin.X = image3.Width / 2f;
                image3.Origin.Y = -7f;
                Image image4 = image2;
                image4.Origin.Y = image4.Origin.Y + 2f;
                list.Add(image3);
            }
            Sprite<int> smallAwardIcon = tower.Stats.GetSmallAwardIcon();
            if (smallAwardIcon != null)
            {
                smallAwardIcon.Play(0, false);
                smallAwardIcon.Origin += new Vector2(10f, 10f);
                list.Add(smallAwardIcon);
            }
        }
        else
        {
            Image image6 = new Image(MapButton.GetSmallBlockTexture(theme.TowerType), null);
            image6.CenterOrigin();
            list.Add(image6);
            if (levelID.Y > 0)
            {
                Image image7 = new Image(MapButton.GetNumeralTexture(theme.TowerType, levelID.Y), null);
                image7.CenterOrigin();
                Image image8 = image7;
                image8.Origin.Y = image8.Origin.Y - 1f;
                image7.Color = MapButton.GetTint(theme.TowerType);
                list.Add(image7);
            }
            Sprite<int> smallAwardIcon2 = tower.Stats.GetSmallAwardIcon();
            if (smallAwardIcon2 != null)
            {
                smallAwardIcon2.Play(0, false);
                smallAwardIcon2.Origin += new Vector2(10f, 3f);
                list.Add(smallAwardIcon2);
            }
        }
        return list;
    }

    public static Image[] InitAdventureTrialsStartLevelGraphics(Point levelID, string levelSet)
    {
        if (levelSet == "TowerFall") 
            return InitTrialsStartLevelGraphics(levelID);
        
        TowerTheme theme = TowerRegistry.TrialsGet(levelSet, levelID.X)[levelID.Y].Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        image2.Color = MapButton.GetTint(theme.TowerType);
        list.Add(image2);
        if (levelID.Y > 0)
        {
            Image image3 = new Image(MapButton.GetNumeralTexture(theme.TowerType, levelID.Y), null);
            image3.Origin.X = image3.Width / 2f;
            image3.Origin.Y = -7f;
            image3.Color = MapButton.GetTint(theme.TowerType);
            Image image4 = image2;
            image4.Origin.Y = image4.Origin.Y + 2f;
            list.Add(image3);
        }
        return list.ToArray();
    }

    public static Image[] InitQuestStartLevelGraphics(int levelID, string levelSet)
    {
        var theme = levelSet == "TowerFall" ? GameData.QuestLevels[levelID].Theme : TowerRegistry.QuestGet(levelSet, levelID).Theme;
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

    public static List<Image> InitAdventureVersusGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
            return new List<Image>();

        TowerTheme theme = TowerRegistry.VersusTowerSets[scene.GetLevelSet()][levelID].Theme;
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        image2.Color = MapButton.GetTint(theme.TowerType);
        return new List<Image> { image, image2 };
    }


    public static List<Image> InitAdventureWorldGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
            return new List<Image>();
        AdventureWorldTowerData worldData = TowerRegistry.DarkWorldTowerSets[scene.GetLevelSet()][levelID];
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

    public static List<Image> InitAdventureQuestGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
            return new List<Image>();

        var tower = ((AdventureQuestTowerData)TowerRegistry.QuestGet(scene.GetLevelSet(), levelID));
        var theme = tower.Theme;
        var stats = tower.Stats;
        var list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        list.Add(image2);
        image2.Color = MapButton.GetTint(theme.TowerType);
        var path = stats switch 
        {
            { CompletedNoDeaths: true } => "questResults/goldSkull",
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

public static class MapButtonExt
{
    public static void SetTitle(this MapButton button, string title) 
    {
        ((patch_MapButton)button).Title = title;
    }
}