using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;
using MonoMod;
using MonoMod.Utils;

namespace FortRise;

public abstract class CustomMapRenderer : CompositeComponent
{
    protected List<GraphicsComponent> Graphics;
    private ICustomMapElement currentElement;
    private Dictionary<string, ICustomMapElement> elements = new();

    public CustomMapRenderer() : base(true, true)
    {
    }

    public override void Added()
    {
        base.Added();
        Graphics = new();
        Graphics = GetList<GraphicsComponent>();
    }

    public abstract void OnSelectionChange(string title);

    public void Set(ICustomMapElement element) 
    {
        currentElement?.OnDeselect();
        currentElement = element;
        currentElement?.OnSelect();
    }

    public MapImage AddImage(Subtexture texture, string name, Action onSelect = null, Action onDeselect = null, Rectangle? clipRect = null) 
    {
        var mapImage = new MapImage(texture) {
            OnSelectImage = onSelect,
            OnDeselectImage = onDeselect
        };
        elements.Add(name, mapImage);
        if (Graphics != null)
            Graphics.Add(mapImage);
        Add(mapImage);
        return mapImage;
    }

    public MapSprite AddAnimatedImage(patch_SpriteData spriteData, string name, Action onSelect = null, Action onDeselect = null, Rectangle? clipRect = null) 
    {
        var spriteString = spriteData.GetSpriteString(name);
        var mapImage = new MapSprite(spriteString.Texture, 60, 60) {
            OnSelectImage = onSelect,
            OnDeselectImage = onDeselect
        };
        mapImage.ClipRect = spriteString.ClipRect;
        DynamicData.For(mapImage).Set("Animations", 
            DynamicData.For(spriteString).Get("Animations"));
        elements.Add(name, mapImage);
        if (Graphics != null)
            Graphics.Add(mapImage);
        return mapImage;
    }

    public override void Render()
    {
        Draw.SineTextureV(TFGame.MenuAtlas["mapWater"], base.Entity.Position, new Vector2(5f, 0f), Vector2.One, 0f, Color.White, XNAGraphics.SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 2f, 1, 0.3926991f);
        base.Render();
    }
}

public class XmlMapRenderer : CustomMapRenderer
{
    public Dictionary<string, ICustomMapElement> ElementMap = new();

    public XmlMapRenderer(string path, string modPath) 
    {
        var map = Calc.LoadXML(path)["map"];
        var containSpriteData = map.HasAttr("spriteData");
        var xmlMapPath = Path.Combine(modPath, map.Attr("atlas"));
        var atlas = AtlasExt.CreateAtlas(null, xmlMapPath + ".xml", xmlMapPath + ".png", ContentAccess.Root);

        patch_SpriteData spriteData = null;
        if (containSpriteData) 
        {
            var spriteDataMapPath = Path.Combine(modPath, map.Attr("spriteData"));
            spriteData = SpriteDataExt.CreateSpriteData(null, spriteDataMapPath + ".xml", atlas, ContentAccess.Root);
        }


        foreach (XmlElement element in map["elements"]) 
        {
            if (element.Name == "mapImage") 
            {
                var name = element.Attr("name");
                var x = element.AttrInt("x");
                var y = element.AttrInt("y");
                patch_Atlas atlasPack;
                if (name.StartsWith("TFGame.MenuAtlas::")) 
                {
                    atlasPack = (patch_Atlas)(object)TFGame.MenuAtlas;
                    name = name.Replace("TFGame.MenuAtlas::", "");
                }
                else
                    atlasPack = atlas;

                AddImage(atlasPack[name], name, () => {}, () => {});
            }
            else if (element.Name == "landImage") 
            {
                var name = element.Attr("name");
                Add(new Image(atlas[name]));
            }
            else if (element.Name == "mapAnimated")
            {
                var name = element.Attr("name");
                if (!containSpriteData && !name.Contains("TFGame.MenuSpriteData::")) 
                {
                    Logger.Error("[CustomMapRenderer] Use of mapAnimated without spriteData is not allowed.");
                    continue;
                }
                var x = element.AttrInt("x");
                var y = element.AttrInt("y");
                var inAnimation = element.ChildText("in", "in");
                var outAnimation = element.ChildText("in", "out");
                var notSelected = element.ChildText("notSelected", "notSelected");
                var selected = element.ChildText("selected", "selected");
                MapSprite animation;
                if (name.StartsWith("TFGame.MenuSpriteData::"))
                    animation = AddAnimatedImage(
                        (patch_SpriteData)(object)TFGame.MenuSpriteData, name.Replace("TFGame.MenuSpriteData::", ""), 
                        () => {}, () => {});
                else
                    animation = AddAnimatedImage(spriteData, name, () => {}, () => {});
                
                animation.OnSelectImage = () => {
                    animation.Play(inAnimation);
                };
                animation.OnDeselectImage = () => {
                    animation.Play(outAnimation);
                };
                animation.Play(notSelected);
                animation.OnAnimationComplete = (s) => {
                    if (s.CurrentAnimID == outAnimation)
                        s.Play(notSelected);
                    if (s.CurrentAnimID == inAnimation)
                        s.Play(selected);
                };
                animation.TowerName = element.ChildText("towerName");
                ElementMap.Add(animation.TowerName, animation);
                Add(animation);
            }
        }
    }

    public override void OnSelectionChange(string title)
    {
        if (ElementMap.TryGetValue(title, out var map)) 
        {
            Set(map);
            return;
        }
        Set(null);
    }
}

public interface ICustomMapElement 
{
    void OnSelect();
    void OnDeselect();
    string TowerName { get; }
}

public class MapImage : Image, ICustomMapElement
{
    public Action OnSelectImage;
    public Action OnDeselectImage;
    public string TowerName { get; set; }
    public MapImage(Texture texture) : base(texture)
    {
    }

    public MapImage(Texture texture, Rectangle? clipRect = null) : base(texture, clipRect)
    {
    }

    public MapImage(Subtexture subTexture, Rectangle? clipRect = null) : base(subTexture, clipRect)
    {
    }

    public virtual void OnDeselect()
    {
        OnDeselectImage?.Invoke();
    }

    public virtual void OnSelect()
    {
        OnSelectImage?.Invoke();
    }
}

public class MapSprite : Sprite<string>, ICustomMapElement
{
    public Action OnSelectImage;
    public Action OnDeselectImage;
    public MapSprite(Texture texture, int frameWidth, int frameHeight, int frameSep = 0) : base(texture, frameWidth, frameHeight, frameSep)
    {
    }

    public MapSprite(Subtexture subTexture, int frameWidth, int frameHeight, int frameSep = 0) : base(subTexture, frameWidth, frameHeight, frameSep)
    {
    }

    public MapSprite(Texture texture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0) : base(texture, clipRect, frameWidth, frameHeight, frameSep)
    {
    }

    public MapSprite(Subtexture subTexture, Rectangle? clipRect, int frameWidth, int frameHeight, int frameSep = 0) : base(subTexture, clipRect, frameWidth, frameHeight, frameSep)
    {
    }

    public string TowerName { get; set; }

    public virtual void OnDeselect()
    {
        OnDeselectImage?.Invoke();
    }

    public virtual void OnSelect()
    {
        OnSelectImage?.Invoke();
    }
}