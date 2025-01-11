using System;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TowerTheme : TowerTheme 
{
    public RiseCore.Resource Mod;
    public patch_TowerTheme(XmlElement xml) {}
    public patch_TowerTheme(XmlElement xml, RiseCore.Resource mod) {}


    [MonoModConstructor]
    public void ctor(XmlElement xml, RiseCore.Resource mod) 
    {
        Mod = mod;
        Name = xml.ChildText("Name").ToUpperInvariant();

        var icon = xml.ChildText("Icon", "sacredGround");

        if (TFGame.MenuAtlas.Contains(icon)) 
        {
            Icon = TFGame.MenuAtlas[icon];
        }
        else 
        {
            Icon = TFGame.MenuAtlas["towerIcons/" + icon];
        }

        TowerType = xml.ChildEnum<MapButton.TowerType>("TowerType", MapButton.TowerType.Normal);
        MapPosition = xml["MapPosition"].Position();
        Music = xml.ChildText("Music", "SacredGround");
        DarknessColor = xml.ChildHexColor("DarknessColor", Color.Black).Invert();
        DarknessOpacity = xml.ChildFloat("DarknessOpacity", 0.2f);
        Wind = xml.ChildInt("Wind", 0);
        Lanterns = xml.ChildEnum<TowerTheme.LanternTypes>("Lanterns", LanternTypes.CathedralTorch);
        World = xml.ChildEnum("World", TowerTheme.Worlds.Normal);
        Raining = xml.ChildBool("Raining", false);
        BackgroundID = xml.ChildText("Background", "SacredGround");
        if (GameData.BGs.ContainsKey(BackgroundID)) 
        {
            BackgroundData = GameData.BGs[this.BackgroundID]["Background"];
            ForegroundData = GameData.BGs[this.BackgroundID]["Foreground"];
        }

        DrillParticleColor = xml.ChildHexColor("DrillParticleColor", Color.Red);
        Cold = xml.ChildBool("Cold", false);
        CrackedBlockColor = xml.ChildHexColor("CrackedBlockColor", "4EB1E9");
        Tileset = xml.ChildText("Tileset", "SacredGround");
        BGTileset = xml.ChildText("BGTileset", "SacredGroundBG");
        Cataclysm = (xml.ChildText("Tileset") == "Cataclysm");

        if (xml.HasChild("PlayerInvisibility"))
        {
            this.InvisibleOpacities = new float[]
            {
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Green", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Blue", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Pink", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Orange", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("White", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Yellow", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Cyan", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Purple", 0) * 0.1f,
                0.2f + (float)xml["PlayerInvisibility"].ChildInt("Red", 0) * 0.1f
            };
            return;
        }

        InvisibleOpacities = new float[]
        {
            0.2f,
            0.2f,
            0.2f,
            0.2f,
            0.2f,
            0.2f,
            0.2f,
            0.2f,
            0.2f
        };
    }

    public Guid GenerateThemeID() 
    {
        return Guid.Empty;
    }
}

public static class TowerThemeExt 
{
    public static bool TryGetMod(this TowerTheme theme, out RiseCore.Resource mod) 
    {
        var moddedTheme = ((patch_TowerTheme)theme);
        if (moddedTheme.Mod != null) 
        {
            mod = moddedTheme.Mod;
            return true;
        }
        mod = null;
        return false;
    }
}