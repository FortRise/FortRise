using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall.Patching;

namespace TowerFall;

public class patch_MapButton : MapButton
{
    [MonoModIgnore]
    public TowerFall.Patching.MapScene Map { get; set; } 
    public string Author { [MonoModIgnore] get; [MonoModIgnore] [MonoModPublic] set; }
    public float TweenAt { get; private set; }


    public string Title { [MonoModIgnore] get => null; [MonoModIgnore][MonoModPublic] set => value = null; }
    


    public patch_MapButton(string title) : base(title)
    {
    }

    [MonoModLinkTo("TowerFall.MapButton", "System.Void .ctor(TowerFall.TowerMapData,System.String)")]
    [MonoModIgnore]
    public void thisctor(TowerMapData data, string title) {}

    protected patch_MapButton(TowerMapData data) : base(data) {}

    [MonoModConstructor]
    [MonoModReplace]
    protected void ctor(TowerMapData data)
    {
        thisctor(data, data.Title);

        if (!string.IsNullOrEmpty(data.Author))
        {
            Author = "BY " + data.Author;
        }
    }


    [MonoModIgnore]
    public extern override void OnConfirm();

    [MonoModIgnore]
    protected extern override List<Image> InitImages();

    [MonoModReplace]
    public static Image[] InitDarkWorldStartLevelGraphics(int levelID)
    {
        var scene = Engine.Instance.Scene as Level;
        if (scene is null)
        {
            return [];
        }

        string levelSet = scene.Session.GetLevelSet();

        TowerTheme theme;
        if (levelSet == "TowerFall")
        {
            theme = GameData.DarkWorldTowers[levelID].Theme;
        }
        else
        {
            theme = TowerRegistry.DarkWorldGet(levelSet, levelID).Theme;
        }

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

    [MonoModReplace]
    public static Image[] InitTrialsStartLevelGraphics(Point levelID)
    {
        var scene = Engine.Instance.Scene as Level;
        if (scene is null)
        {
            return [];
        }

        string levelSet = scene.Session.GetLevelSet();

        TowerTheme theme;
        if (levelSet == "TowerFall")
        {
            theme = GameData.TrialsLevels[levelID.X, levelID.Y].Theme;
        }
        else
        {
            theme = TowerRegistry.TrialsGet(levelSet, levelID.X)[levelID.Y].Theme;
        }

        List<Image> list = [];
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType));
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        image2.Color = MapButton.GetTint(theme.TowerType);
        list.Add(image2);

        if (levelID.Y > 0)
        {
            Image image3 = new Image(MapButton.GetNumeralTexture(theme.TowerType, levelID.Y));
            image3.Origin.X = image3.Width / 2f;
            image3.Origin.Y = -7f;
            image3.Color = MapButton.GetTint(theme.TowerType);
            Image image4 = image2;
            image4.Origin.Y = image4.Origin.Y + 2f;
            list.Add(image3);
        }

        return list.ToArray();
    }

    [MonoModReplace]
    public static Image[] InitQuestStartLevelGraphics(int levelID)
    {
        var scene = Engine.Instance.Scene as Level;
        if (scene is null)
        {
            return [];
        }

        string levelSet = scene.Session.GetLevelSet();

        TowerTheme theme;
        if (levelSet == "TowerFall")
        {
            theme = GameData.QuestLevels[levelID].Theme;
        }
        else
        {
            theme = TowerRegistry.QuestGet(levelSet, levelID).Theme;
        }

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

    [MonoModReplace]
    public static List<Image> InitVersusGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
        {
            return [];
        }
        string towerSet = scene.TowerSet;

        TowerTheme theme;
        if (towerSet == "TowerFall")
        {
            theme = GameData.VersusTowers[levelID].Theme;
        }
        else
        {
            VersusTowerData worldData = TowerRegistry.VersusTowerSets[towerSet][levelID];
            theme = worldData.Theme;
        }

        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        image2.Color = MapButton.GetTint(theme.TowerType);
        return [image, image2];
    }

    [MonoModReplace]
    public static List<Image> InitQuestGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
        {
            return [];
        }

        string towerSet = scene.TowerSet;

        QuestLevelData tower;
        QuestTowerStats stats;
        if (towerSet == "TowerFall")
        {
            tower = GameData.QuestLevels[levelID];
            stats = SaveData.Instance.Quest.Towers[levelID];
        }
        else
        {
            tower = TowerRegistry.QuestTowerSets[towerSet][levelID];
            stats = FortRiseModule.SaveData.AdventureQuest.AddOrGet(tower.GetLevelID());
        }


        TowerTheme theme = tower.Theme;
        List<Image> list = new List<Image>();
        Image image = new Image(MapButton.GetBlockTexture(theme.TowerType), null);
        image.CenterOrigin();
        list.Add(image);
        Image image2 = new Image(theme.Icon, null);
        image2.CenterOrigin();
        list.Add(image2);
        image2.Color = MapButton.GetTint(theme.TowerType);
        if (!tower.IsOfficialLevelSet() || stats.Revealed)
        { 
            if (stats.CompletedNoDeaths)
            {
                Image image3 = new Image(TFGame.MenuAtlas["questResults/goldSkull"], null);
                image3.CenterOrigin();
                image3.Origin += new Vector2(10f, 10f);
                list.Add(image3);
            }
            else if (stats.CompletedHardcore)
            {
                Image image4 = new Image(TFGame.MenuAtlas["questResults/redSkull"], null);
                image4.CenterOrigin();
                image4.Origin += new Vector2(10f, 10f);
                list.Add(image4);
            }
            else if (stats.CompletedNormal)
            {
                Image image5 = new Image(TFGame.MenuAtlas["questResults/whiteSkull"], null);
                image5.CenterOrigin();
                image5.Origin += new Vector2(10f, 10f);
                list.Add(image5);
            }
        }
        return list;
    }

    [MonoModReplace]
    public static List<Image> InitTrialsGraphics(Point levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as TowerFall.MapScene;
        if (scene == null)
        {
            return [];
        }

        string towerSet = scene.TowerSet;

        TrialsLevelData tower;
        if (towerSet == "TowerFall")
        {
            tower = GameData.TrialsLevels[levelID.X, levelID.Y];
        }
        else
        {
            tower = TowerRegistry.TrialsGet(towerSet, levelID.X, levelID.Y);
        }

        TowerTheme theme = tower.Theme;
        List<Image> list = new List<Image>();
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
            Sprite<int> smallAwardIcon = GetSmallAwardIcon();
            if (smallAwardIcon != null)
            {
                smallAwardIcon.Play(0, false);
                smallAwardIcon.Origin += new Vector2(10f, 10f);
                list.Add(smallAwardIcon);
            }
            if (tower.IsOfficialLevelSet() && SaveData.Instance.Unlocks.YellowGemsFound.Contains(levelID))
            {
                Image image5 = new Image(TFGame.MenuAtlas["trials/yellowGem"], null);
                image5.CenterOrigin();
                image5.Origin += new Vector2(-10f, 10f);
                list.Add(image5);
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
            Sprite<int> smallAwardIcon2 = GetSmallAwardIcon();
            if (smallAwardIcon2 != null)
            {
                smallAwardIcon2.Play(0, false);
                smallAwardIcon2.Origin += new Vector2(10f, 3f);
                list.Add(smallAwardIcon2);
            }
            if (tower.IsOfficialLevelSet() && SaveData.Instance.Unlocks.YellowGemsFound.Contains(levelID))
            {
                Image image9 = new Image(TFGame.MenuAtlas["trials/yellowGem"], null);
                image9.CenterOrigin();
                image9.Origin += new Vector2(-10f, 3f);
                list.Add(image9);
            }
        }
        return list;

        Sprite<int> GetSmallAwardIcon()
        {
            if (tower.IsOfficialLevelSet())
            {
                return SaveData.Instance.Trials.Levels[levelID.X][levelID.Y].GetSmallAwardIcon();
            }

            return FortRiseModule.SaveData.AdventureTrials.AddOrGet(tower.GetLevelID()).GetSmallAwardIcon();
        }
    }


    [MonoModReplace]
    public static List<Image> InitDarkWorldGraphics(int levelID)
    {
        // We don't have access to the MapScene from MapButton yet.
        var scene = Engine.Instance.Scene as MapScene;
        if (scene == null)
        {
            return [];
        }

        string towerSet = scene.TowerSet;

        TowerTheme theme;
        DarkWorldTowerStats stats;
        if (towerSet == "TowerFall")
        {
            theme = GameData.DarkWorldTowers[levelID].Theme;
            stats = SaveData.Instance.DarkWorld.Towers[levelID];
        }
        else
        {
            DarkWorldTowerData worldData = TowerRegistry.DarkWorldTowerSets[towerSet][levelID];
            stats = FortRiseModule.SaveData.AdventureWorld.AddOrGet(worldData.GetLevelID());
            theme = worldData.Theme;
        }


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

public static class MapButtonExt
{
    public static void SetTitle(this MapButton button, string title) 
    {
        ((patch_MapButton)button).Title = title;
    }
}
