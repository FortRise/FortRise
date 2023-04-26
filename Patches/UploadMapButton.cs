#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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
        using FolderBrowserDialog fileDialog = new FolderBrowserDialog();


        if (fileDialog.ShowDialog() != DialogResult.Cancel && !string.IsNullOrEmpty(fileDialog.SelectedPath)) 
        {
            var selectedPath = fileDialog.SelectedPath.Replace("\\", "/");
            if (!patch_GameData.AdventureWorldTowersLoaded.Contains(selectedPath) && 
                patch_GameData.LoadAdventureLevels(selectedPath, true))
            {
                patch_GameData.AdventureWorldTowersLoaded.Add(selectedPath);
                var jArray = TeuJson.JsonUtility.ConvertToJsonArray(patch_GameData.AdventureWorldTowersLoaded)
                    .ToString(JsonTextWriterOptions.Default);
                using var fs = File.Create("adventureCache.json");
                using TextWriter tw = new StreamWriter(fs);
                tw.Write(jArray);
            }
        }
        Map.Selection = null;
        OnDeselect();
        Map.GotoAdventure();
        Map.MatchStarting = false;
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