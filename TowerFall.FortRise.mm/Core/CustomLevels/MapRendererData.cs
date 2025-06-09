using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class MapRendererNode : CompositeComponent
{
    public XmlElement Xml;
    public Subtexture Water;
    public Subtexture Land;
    public bool HideVanilla;

    public Dictionary<string, AnimatedTower> AnimatedTowers = new();
    public AnimatedTower CurrentTower;

    public MapRendererNode(XmlElement xml) : base(true, true)
    {
        Xml = xml;
        foreach (XmlElement element in xml["elements"]) 
        {
            switch (element.Name) 
            {
            case "hideVanilla":
                HideVanilla = true;
                break;
            case "landImage":
                var landImageName = element.Attr("image", "mapLand");
                Land = TFGame.MenuAtlas[landImageName];
                Add(new Image(Land));
                break;
            case "waterImage":
                var waterImageName = element.Attr("image", "mapWater");
                Water = TFGame.MenuAtlas[waterImageName];
                break;
            case "mapImage":
                var mapImageName = element.Attr("image");
                var x = element.AttrInt("x");
                var y = element.AttrInt("y");
                
                Add(new Image(TFGame.MenuAtlas[mapImageName]));
                break;
            case "mapAnimated":
                var mapAnimated = element.Attr("name");
                
                var mapX = element.AttrInt("x");
                var mapY = element.AttrInt("y");
                var inAnimation = element.ChildText("in", "in");
                var outAnimation = element.ChildText("out", "out");
                var notSelected = element.ChildText("notSelected", "notSelected");
                var selected = element.ChildText("selected", "selected");
                Sprite<string> animation = TFGame.MenuSpriteData.GetSpriteString(mapAnimated);
                
                var tower = new AnimatedTower(inAnimation, outAnimation, selected, notSelected, animation);

                animation.Position = new Vector2(mapX, mapY);
                
                animation.Play(notSelected);
                animation.OnAnimationComplete = (s) => {
                    if (s.CurrentAnimID == outAnimation)
                        s.Play(notSelected);
                    if (s.CurrentAnimID == inAnimation)
                        s.Play(selected);
                };

                AddAnimatedTowers(element.ChildText("towerName", ""), tower);
                break;
            }
        }
    }

    public void StartSelection(string towerName) 
    {
        if (AnimatedTowers.TryGetValue(towerName, out var value)) 
        {
            if (CurrentTower == value)
            {
                return;
            }
            
            value.Select();
            CurrentTower?.DeSelect();
            CurrentTower = value;
            return;
        }
        CurrentTower?.DeSelect();
        CurrentTower = null;
    }

    public void Deselection() 
    {
        CurrentTower?.DeSelect();
        CurrentTower = null;
    }

    private void AddAnimatedTowers(string towerTarget, AnimatedTower tower) 
    {
        ref var tow = ref CollectionsMarshal.GetValueRefOrAddDefault(AnimatedTowers, towerTarget, out bool exists);
        if (exists)
        {
            return; 
        }

        tow = tower;
        Add(tower.Sprite);
    }

    public class AnimatedTower 
    {
        public string In;
        public string Out;
        public string Selected;
        public string NotSelected;
        public Sprite<string> Sprite;

        public AnimatedTower(string ins, string outs, string selected, string notSelected, Sprite<string> sprite) 
        {
            In = ins;
            Out = outs;
            Selected = selected;
            NotSelected = notSelected;
            Sprite = sprite;
        }

        public void Select() 
        {
            Sprite.Play(In);
        }

        public void DeSelect() 
        {
            Sprite.Play(Out);
        }
    }
}