using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class MapRendererData : CompositeComponent
{
    public Subtexture Water;
    public Subtexture Land;
    public Vector2 Size;
    public bool HideVanilla;

    public Dictionary<string, AnimatedTower> AnimatedTowers = [];
    public AnimatedTower CurrentTower;

    public MapRendererData(in MapRendererConfiguration configuration)
        : base(true, true)
    {
        Water = configuration.Water.Subtexture;
        Land = configuration.Land.Subtexture;

        int width = Land.Width;
        int height = Land.Height;

        if (configuration.Width.TryGetValue(out int w))
        {
            width = w;
        }

        if (configuration.Height.TryGetValue(out int h))
        {
            height = h;
        }

        Size = new Vector2(width, height);

        Add(new Image(Land));

        foreach (var elm in configuration.Elements)
        {
            elm.Sprite.Switch(
                subtexture => {
                    float x = elm.Position.X;
                    float y = elm.Position.Y;
                    Add(new Image(subtexture.Subtexture) { Position = new Vector2(x, y) });
                },
                data => {
                    var inAnimation = data.In;
                    var outAnimation = data.Out;
                    var notSelected = data.NotSelected;
                    var selected = data.Selected;

                    var sprite = data.Sprite.GetCastEntry<string>().Sprite;
                    sprite.Position = new Vector2(elm.Position.X, elm.Position.Y);

                    sprite.Play(notSelected);
                    sprite.OnAnimationComplete = (s) => {
                        if (s.CurrentAnimID == outAnimation)
                        {
                            s.Play(notSelected);
                        }

                        if (s.CurrentAnimID == inAnimation)
                        {
                            s.Play(selected);
                        }
                    };

                    Add(sprite);
                }
            );
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
