using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Monocle;
using TeuJson;

namespace TowerFall;

public sealed class UploadMapButton : patch_MapButton
{
    public UploadMapButton() : base("UPLOAD LEVELS")
    {
    }

    public override void OnConfirm()
    {
        LoadTower();
        Map.Selection = null;
        OnDeselect();
        Map.GotoAdventure();
        Map.MatchStarting = false;
    }

    private void LoadTower() 
    {
        if (XNAFileDialog.ShowDialogSynchronous("Load DarkWorld Tower .xml file") && !string.IsNullOrEmpty(XNAFileDialog.Path)) 
        {
            Load(Path.GetDirectoryName(XNAFileDialog.Path));
        }
    }

    private void Load(string path) 
    {
        var selectedPath = path.Replace("\\", "/");
        if (!patch_GameData.AdventureWorldTowersLoaded.Contains(selectedPath) && 
            patch_GameData.LoadAdventureLevelsParallel(selectedPath))
        {
            patch_GameData.AdventureWorldTowersLoaded.Add(selectedPath);
            SaveLoaded();
        }
    }

    internal static void SaveLoaded() 
    {
        var jArray = TeuJson.JsonUtility.ConvertToJsonArray(patch_GameData.AdventureWorldTowersLoaded)
            .ToString(JsonTextWriterOptions.Default);
        using var fs = File.Create("adventureCache.json");
        using TextWriter tw = new StreamWriter(fs);
        tw.Write(jArray);
    }

    protected override List<Image> InitImages()
    {
        Image image = new Image(TFGame.MenuAtlas["randomLevelBlock"], null);
        image.CenterOrigin();
        Image image2 = new Image(TFGame.MenuAtlas["towerIcons/discovery"], null);
        image2.CenterOrigin();
        image2.Color = Color.Gray;
        return new List<Image>
        {
            image,
            image2
        };
    }
}