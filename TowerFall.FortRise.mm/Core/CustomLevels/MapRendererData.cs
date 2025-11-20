using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

#nullable enable
public class MapRendererData : CompositeComponent
{
    public Subtexture? Water { get; private set; }
    public Subtexture? Land { get; private set; }
    public Vector2 Size { get; private set; }
    public bool HideVanilla { get; private set; }

    private Dictionary<string, AnimatedTower> animatedTowers = [];
    private AnimatedTower? currentTower;

    public MapRendererData(in MapRendererConfiguration configuration)
        : base(true, true)
    {
        Subtexture land;
        if (configuration.Land is null)
        {
            land = TFGame.MenuAtlas["mapLand"];
        }
        else
        {
            land = configuration.Land.Subtexture!;
        }

        Subtexture water;
        if (configuration.Water is null)
        {
            water = TFGame.MenuAtlas["mapWater"];
        }
        else
        {
            water = configuration.Water.Subtexture!;
        }
        Water = water;
        Land = land;
        HideVanilla = configuration.HideVanillaElements;

        int width = land.Width;
        int height = land.Height;

        if (configuration.Width.TryGetValue(out int w))
        {
            width = w;
        }

        if (configuration.Height.TryGetValue(out int h))
        {
            height = h;
        }

        Size = new Vector2(width, height);

        if (Land is not null)
        {
            Add(new Image(Land));
        }

        foreach (var elm in configuration.Elements)
        {
            elm.Sprite.Switch(
                subtexture => {
                    float x = elm.Position.X;
                    float y = elm.Position.Y;
                    Add(new Image(subtexture.Subtexture) { Position = new Vector2(x, y) });
                },
                data => {
                    var notSelected = data.NotSelected;
                    var selected = data.Selected;

                    var inAnimation = data.In ?? selected;
                    var outAnimation = data.Out ?? notSelected;

                    var sprite = data.Sprite.GetCastEntry<string>().Sprite!;
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

                    if (data.TowerID is not null)
                    {
                        animatedTowers[data.TowerID] = new AnimatedTower(
                            inAnimation,
                            outAnimation,
                            selected,
                            notSelected,
                            sprite
                        );
                    }
                }
            );
        }
    }

    public void StartSelection(string towerName) 
    {
        if (animatedTowers.TryGetValue(towerName, out var value)) 
        {
            if (currentTower == value)
            {
                return;
            }
            
            value.Select();
            currentTower?.DeSelect();
            currentTower = value;
            return;
        }
        currentTower?.DeSelect();
        currentTower = null;
    }

    public void Deselection() 
    {
        currentTower?.DeSelect();
        currentTower = null;
    }

    private class AnimatedTower(string ins, string outs, string selected, string notSelected, Sprite<string> sprite)
    {
        public string In = ins;
        public string Out = outs;
        public string Selected = selected;
        public string NotSelected = notSelected;
        public Sprite<string> Sprite = sprite;

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
