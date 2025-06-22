#nullable enable
using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise;

internal class SpriteEntry<T> : ISpriteEntry<T>
{
    public string ID { get; init; }
    public SpriteConfiguration<T> Configuration { get; init; }
    public Sprite<T>? Sprite => GetActualSprite();

    public XmlElement Xml => xmlCallback();
    private XmlElement? cache;
    private Func<XmlElement> xmlCallback;

    public SpriteEntry(string id, SpriteConfiguration<T> configuration)
    {
        ID = id;
        Configuration = configuration;
        xmlCallback = GetActualXml;
    }

    public SpriteEntry(string id, Func<XmlElement> callback)
    {
        ID = id;
        xmlCallback = callback;
    }

    private XmlElement GetActualXml()
    {
        if (cache != null)
        {
            return cache;
        }
        var document = new XmlDocument();
        var type = typeof(T);
        string spriteType;


        if (type == typeof(int))
        {
            spriteType = "sprite_int";
        }
        else if (type == typeof(string))
        {
            spriteType = "sprite_string";
        }
        else
        {
            throw new Exception($"[SpriteEntry] Unsupported type: {typeof(T).Name}");
        }
        var sprite = document.CreateElement(spriteType);
        sprite.SetAttribute("id", ID);
        CreateProperty(document, sprite, "Texture", Configuration.Texture.ID);
        CreateProperty(document, sprite, "FrameWidth", Configuration.FrameWidth);
        CreateProperty(document, sprite, "FrameHeight", Configuration.FrameHeight);
        CreateProperty(document, sprite, "OriginX", Configuration.OriginX);
        CreateProperty(document, sprite, "OriginY", Configuration.OriginY);
        CreateProperty(document, sprite, "X", Configuration.X);
        CreateProperty(document, sprite, "Y", Configuration.Y);

        if (Configuration.RedTexture != null)
        {
            CreateProperty(document, sprite, "RedTexture", Configuration.RedTexture.ID);
        }

        if (Configuration.RedTeam != null)
        {
            CreateProperty(document, sprite, "RedTeam", Configuration.RedTeam.ID);
        }

        if (Configuration.BlueTeam != null)
        {
            CreateProperty(document, sprite, "BlueTeam", Configuration.BlueTeam.ID);
        }

        if (Configuration.BlueTexture != null)
        {
            CreateProperty(document, sprite, "BlueTexture", Configuration.BlueTexture.ID);
        }

        if (Configuration.Flash != null)
        {
            CreateProperty(document, sprite, "Flash", Configuration.Flash.ID);
        }

        if (Configuration.HeadYOrigins != null)
        {
            string headYorigins = string.Join(',', Configuration.HeadYOrigins);
            CreateProperty(document, sprite, "HeadYOrigins", headYorigins);
        }

        var animations = document.CreateElement("Animations");
        for (int i = 0; i < Configuration.Animations.Length; i++)
        {
            var anim = Configuration.Animations[i];
            var elm = document.CreateElement("Anim");
            elm.SetAttribute("id", anim.ID!.ToString());

            string frames = string.Join(',', anim.Frames);
            elm.SetAttribute("frames", frames);
            elm.SetAttribute("delay", anim.Delay.ToString());
            elm.SetAttribute("loop", anim.Loop ? "True" : "False");
            animations.AppendChild(elm);
        }

        if (Configuration.AdditionalData != null)
        {
            foreach (var additional in Configuration.AdditionalData)
            {
                CreateProperty(document, sprite, additional.Key, additional.Value);
            }
        }

        sprite.AppendChild(animations);

        return sprite;
    }

    private XmlElement CreateProperty<T1>(XmlDocument document, XmlElement parent, string propertyName, T1 value)
    where T1: notnull
    {
        XmlElement element = document.CreateElement(propertyName);
        element.InnerText = value.ToString()!;
        parent.AppendChild(element);
        return element;
    }

    private Sprite<T> GetActualSprite()
    {
        if (Configuration.Texture == null)
        {
            throw new Exception($"[SpriteEntry] The Sprite that has been used might have been a Vanilla Sprite. Use 'TFGame.SpriteData' or any of its variant instead.");
        }

        if (typeof(T) == typeof(string))
        {
            XmlElement xml = Xml;

            Sprite<string> sprite = new Sprite<string>(Configuration.Texture.Subtexture, xml.ChildInt("FrameWidth"), xml.ChildInt("FrameHeight"), 0);
            sprite.Origin = new Vector2(xml.ChildFloat("OriginX", 0f), xml.ChildFloat("OriginY", 0f));
            sprite.Position = new Vector2(xml.ChildFloat("X", 0f), xml.ChildFloat("Y", 0f));
            sprite.Color = xml.ChildHexColor("Color", Color.White);

            XmlElement animationXml = xml["Animations"]!;
            if (animationXml != null)
            {
                foreach (XmlElement animXml in animationXml.GetElementsByTagName("Anim"))
                {
                    sprite.Add(
                        animXml.Attr("id"),
                        animXml.AttrFloat("delay", 0f),
                        animXml.AttrBool("loop", true),
                        Calc.ReadCSVInt(animXml.Attr("frames"))
                    );
                }
            }
            return (Sprite<T>)(object)sprite;
        }
        else if (typeof(T) == typeof(int))
        {
            XmlElement xml = Xml;

            Sprite<int> sprite = new Sprite<int>(Configuration.Texture.Subtexture, xml.ChildInt("FrameWidth"), xml.ChildInt("FrameHeight"), 0);
            sprite.Origin = new Vector2(xml.ChildFloat("OriginX", 0f), xml.ChildFloat("OriginY", 0f));
            sprite.Position = new Vector2(xml.ChildFloat("X", 0f), xml.ChildFloat("Y", 0f));
            sprite.Color = xml.ChildHexColor("Color", Color.White);

            XmlElement animationXml = xml["Animations"]!;
            if (animationXml != null)
            {
                foreach (XmlElement animXml in animationXml.GetElementsByTagName("Anim"))
                {
                    sprite.Add(
                        animXml.AttrInt("id"),
                        animXml.AttrFloat("delay", 0f),
                        animXml.AttrBool("loop", true),
                        Calc.ReadCSVInt(animXml.Attr("frames"))
                    );
                }
            }
            return (Sprite<T>)(object)sprite;
        }
        else
        {
            throw new Exception($"[SpriteEntry] Unsupported type: {typeof(T).Name}");
        }
    }
}
