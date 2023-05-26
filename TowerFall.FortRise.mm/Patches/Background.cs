using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_Background : Background
{
    private Level level;
    private List<Background.BGElement> elements;
    public patch_Background(Level level, XmlElement xml) : base(level, xml)
    {
    }


    [MonoModReplace]
    private void ReadLayers(XmlElement xml) 
    {
        var lookup = false;
        foreach (var obj in xml) 
        {
            if (obj is XmlElement xmlElement) 
            {
                var name = xmlElement.Name;
                switch (name) 
                {
                case "Overtime":
                    if (this.level != null && this.level.Session.IsInOvertime)
                    {
                        this.ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    lookup = true;
                    break;
                case "Else":
                    if (lookup) 
                    {
                        ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    break;
                case "GhostShipLayer":
                    elements.Add(new Background.GhostShipLayer(level, xmlElement));
                    break;
                case "FadeMoveLayer":
                    elements.Add(new Background.FadeMoveLayer(level, xmlElement));
                    break;
                case "WavyLayer":
                    elements.Add(new Background.WavyLayer(level, xmlElement));
                    break;
                case "Random":
                    if (Calc.Random.Chance(xmlElement.AttrFloat("chance", 0.5f))) 
                    {
                        ReadLayers(xmlElement);
                        lookup = false;
                        break;
                    }
                    lookup = true;
                    break;
                case "AscensionLightningLayer":
                    elements.Add(new Background.AscensionLightningLayer(level, xmlElement));
                    break;
                case "LightningFlash":
                    if (!SaveData.Instance.Options.RemoveScreenFlashEffects) 
                    {
                        elements.Add(new Background.LightningFlashLayer(level, xmlElement));
                        break;
                    }
                    break;
                case "FadeLayer":
                    elements.Add(new Background.FadeLayer(level, xmlElement));
                    break;
                case "ScrollLayer":
                    elements.Add(new Background.ScrollLayer(level, xmlElement));
                    break;
                case "NodeLayer":
                    elements.Add(new Background.NodeLayer(level, xmlElement));
                    break;
                case "OceanLayer":
                    elements.Add(new Background.OceanLayer(level, xmlElement));
                    break;
                case "AnimatedLayer":
                    elements.Add(new Background.AnimatedLayer(level, xmlElement));
                    break;
                case "Backdrop":
                    elements.Add(new Background.Backdrop(level, xmlElement));
                    break;
                case "CustomBackdrop":
                    elements.Add(new patch_Background.CustomBackdrop(level, xmlElement));
                    break;
                case "VortexLayer":
                    elements.Add(new Background.VortexLayer(level, xmlElement));
                    break;
                case "FlightMoonLayer":
                    elements.Add(new Background.FlightMoonLayer(level, xmlElement));
                    break;
                case "SacredGroundMoonLayer":
                    elements.Add(new Background.SacredGroundMoonLayer(level, xmlElement));
                    break;
                }
            }
        }
    }

    public class CustomBackdrop : Background.BGElement
    {
        public Image Image;
        public CustomBackdrop(Level level, XmlElement xml) : base(level)
        {
            this.Image = new Image(patch_GameData.CustomBGAtlas[xml.InnerText], null);
            this.Image.Position = xml.Position(Vector2.Zero);
            this.Image.Color = Color.White * xml.AttrFloat("opacity", 1f);
        }

        public override void Render()
        {
            Image.Render();
        }

        public override void Update()
        {
        }
    }
}
