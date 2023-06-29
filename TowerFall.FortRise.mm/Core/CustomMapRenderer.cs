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
    private Subtexture mapWater;

    public CustomMapRenderer() : base(true, true)
    {
    }


    public override void Added()
    {
        base.Added();
        Graphics = new();
        Graphics = GetList<GraphicsComponent>();
        if (mapWater == null)
            mapWater = TFGame.MenuAtlas["mapWater"];
    }

    public abstract void OnSelectionChange(string title);

    public void Set(ICustomMapElement element) 
    {
        currentElement?.OnDeselect();
        currentElement = element;
        currentElement?.OnSelect();
    }

    public void SetWaterImage(Subtexture water) 
    {
        mapWater = water;
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
        var mapImage = new MapSprite(spriteString.Texture, spriteString.ClipRect, 
            spriteString.FrameRects[0].Width, spriteString.FrameRects[0].Height) {
            Origin = spriteString.Origin,
            Color = spriteString.Color,
            OnSelectImage = onSelect,
            OnDeselectImage = onDeselect
        };
        DynamicData.For(mapImage).Set("Animations", 
            DynamicData.For(spriteString).Get("Animations"));
        elements.Add(name, mapImage);
        if (Graphics != null)
            Graphics.Add(mapImage);
        return mapImage;
    }

    public override void Render()
    {
        Draw.SineTextureV(mapWater, base.Entity.Position, new Vector2(5f, 0f), Vector2.One, 0f, Color.White, XNAGraphics.SpriteEffects.None, base.Scene.FrameCounter * 0.03f, 2f, 1, 0.3926991f);
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
            switch (element.Name)
            {
            case "mapImage":
                var mapImageName = element.Attr("name");
                var x = element.AttrInt("x");
                var y = element.AttrInt("y");
                patch_Atlas atlasPack;
                if (mapImageName.StartsWith("TFGame.MenuAtlas::")) 
                {
                    atlasPack = (patch_Atlas)(object)TFGame.MenuAtlas;
                    mapImageName = mapImageName.Replace("TFGame.MenuAtlas::", "");
                }
                else
                    atlasPack = atlas;
                
                var image = AddImage(atlasPack[mapImageName], mapImageName, () => {}, () => {});
                image.Position = new Vector2(x, y);
                break;
            case "landImage":
                var landImageName = element.Attr("name");
                Add(new Image(atlas[landImageName]));
                break;
            case "waterImage":
                var waterImageName = element.Attr("name");
                SetWaterImage(atlas[waterImageName]);
                break;
            case "mapAnimated":
                var mapAnimated = element.Attr("name");
                if (!containSpriteData && !mapAnimated.Contains("TFGame.MenuSpriteData::")) 
                {
                    Logger.Error("[CustomMapRenderer] Use of mapAnimated without spriteData is not allowed.");
                    continue;
                }
                var mapX = element.AttrInt("x");
                var mapY = element.AttrInt("y");
                var inAnimation = element.ChildText("in", "in");
                var outAnimation = element.ChildText("out", "out");
                var notSelected = element.ChildText("notSelected", "notSelected");
                var selected = element.ChildText("selected", "selected");
                MapSprite animation;
                if (mapAnimated.StartsWith("TFGame.MenuSpriteData::"))
                    animation = AddAnimatedImage(
                        (patch_SpriteData)(object)TFGame.MenuSpriteData, mapAnimated.Replace("TFGame.MenuSpriteData::", ""), 
                        () => {}, () => {});
                else
                    animation = AddAnimatedImage(spriteData, mapAnimated, () => {}, () => {});

                animation.Position = new Vector2(mapX, mapY);
                
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
                break;
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