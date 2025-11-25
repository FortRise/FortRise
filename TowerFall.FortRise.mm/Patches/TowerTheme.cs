using System;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TowerTheme : TowerTheme 
{
    public string ID; 
    public patch_TowerTheme() {}
    public patch_TowerTheme(XmlElement xml) {}
    public patch_TowerTheme(XmlElement xml, IResourceInfo mod) {}

    public extern void orig_ctor(XmlElement xml);

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor()
    { 
        CrackedBlockColor = Calc.HexToColor("4EB1E9");
        InvisibleOpacities = [0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f];
    }

    [MonoModConstructor]
    public void ctor(XmlElement xml) 
    {
        orig_ctor(xml);
        ID = xml.Attr("id", Name);
    }


    [MonoModConstructor]
    public void ctor(XmlElement xml, IResourceInfo mod) 
    {
        Name = xml.ChildText("Name").ToUpperInvariant();
        ID = mod.Root.Substring(4) + xml.Attr("id", Name);

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
        if (GameData.BGs.TryGetValue(BackgroundID, out XmlElement value)) 
        {
            BackgroundData = value["Background"];
            ForegroundData = value["Foreground"];
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
}