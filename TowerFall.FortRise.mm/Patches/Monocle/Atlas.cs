#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;

namespace Monocle;

public class patch_Atlas : Atlas
{
    private string xmlPath;
    internal HashSet<string> injectedAtlas = new HashSet<string>();

    public patch_Atlas(string xmlPath, string imagePath, bool load) : base(xmlPath, imagePath, load) { }

    public patch_Atlas() : base(null, null, false) { }

    public extern void orig_ctor(string xmlPath, string imagePath, bool load);

    [MonoModConstructor]
    public void ctor(string xmlPath, string imagePath, bool load)
    {
        SafeSubTextures = [];
        injectedAtlas = [];
        orig_ctor(xmlPath, imagePath, load);
    }


    public Dictionary<string, Subtexture> SubTextures { get; private set; }

    [Obsolete("Use SubTextures instead")]
    internal ConcurrentDictionary<string, Subtexture> SafeSubTextures { get; private set; }

    public Subtexture this[string name]
    {
        [MonoModReplace]
        get
        {
            ref var texture = ref CollectionsMarshal.GetValueRefOrNullRef(SubTextures, name);
            if (!Unsafe.IsNullRef(ref texture))
            {
                return texture;
            }

            return SafeSubTextures.GetValueOrDefault(name);
        }
        set
        {
            SubTextures[name] = value;
        }
    }

    public void SafeAdd(string name, Subtexture texture)
    {
        SafeSubTextures[name] = texture;
    }

    internal void ConvertToFastLookup()
    {
        foreach (var concurrent in SafeSubTextures)
        {
            SubTextures[concurrent.Key] = concurrent.Value;
        }

        SafeSubTextures.Clear();
    }

    internal void SetXMLPath(string xmlPath)
    {
        this.xmlPath = xmlPath;
    }

    internal void SetImagePath(string imagePath)
    {
        this.ImagePath = imagePath;
    }

    internal void SetSubTextures(Dictionary<string, Subtexture> subTextures)
    {
        this.SubTextures = subTextures;
    }


    [MonoModConstructor]
    internal void ctor()
    {
        injectedAtlas = new HashSet<string>();
    }

    [MonoModIgnore]
    public extern bool Contains(string name);

    internal void LoadStream(Stream fs)
    {
        Texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
        Rect = new Rectangle(0, 0, this.Texture2D.Width, this.Texture2D.Height);
    }
}

public static class AtlasExt
{
    internal static void SafeAdd(this Atlas atlas, string id, Subtexture tex)
    {
        ((patch_Atlas)atlas).SafeAdd(id, tex);
    }

    internal static patch_Atlas CreateAtlasFromEmbedded(string xmlPath, string imagePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream xmlStream = assembly.GetManifestResourceStream(xmlPath);
        using Stream imageStream = assembly.GetManifestResourceStream(imagePath);

        using var streamReader = new StreamReader(xmlStream);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(streamReader.ReadToEnd());

        XmlNodeList elementsByTagName = xmlDocument["TextureAtlas"].GetElementsByTagName("SubTexture");
        var atlas = new patch_Atlas();

        atlas.SetXMLPath(xmlPath);
        atlas.SetImagePath(imagePath);
        atlas.SetSubTextures(new Dictionary<string, Subtexture>(elementsByTagName.Count));
        foreach (XmlElement item in elementsByTagName)
        {
            XmlAttributeCollection attributes = item.Attributes;
            atlas.SubTextures.Add(attributes["name"].Value, new Subtexture(atlas, Convert.ToInt32(attributes["x"].Value), Convert.ToInt32(attributes["y"].Value), Convert.ToInt32(attributes["width"].Value), Convert.ToInt32(attributes["height"].Value)));
        }
        atlas.LoadStream(imageStream);

        return atlas;
    }

    public static patch_Atlas CreateAtlas(string xmlPath, string imagePath)
    {
        using var rootXmlStream = ModIO.OpenRead(xmlPath);
        using var rootImageStream = ModIO.OpenRead(imagePath);
        return AtlasExt.CreateAtlas(rootXmlStream, rootImageStream);
    }

    public static patch_Atlas CreateAtlas(Stream xmlStream, Stream imageStream)
    {
        patch_Atlas atlas = AtlasReader.Read(xmlStream, ".xml");
        atlas.LoadStream(imageStream);
        return atlas;
    }

    public static patch_Atlas CreateAtlas(Stream xmlStream, Stream imageStream, string ext)
    {
        patch_Atlas atlas = AtlasReader.Read(xmlStream, ext);
        atlas.LoadStream(imageStream);
        return atlas;
    }

    public static patch_Atlas CreateAtlasJson(Stream jsonStream, Stream imageStream)
    {
        patch_Atlas atlas = AtlasReader.Read(jsonStream, ".json");
        atlas.LoadStream(imageStream);
        return atlas;
    }

    internal static HashSet<string> GetAllInjectedAtlas(this Atlas atlas)
    {
        return ((patch_Atlas)atlas).injectedAtlas;
    }
}
