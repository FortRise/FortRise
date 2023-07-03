using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;
using TowerFall;

namespace Monocle;

public class patch_Atlas : Atlas
{
    private string xmlPath;

    public patch_Atlas(string xmlPath, string imagePath, bool load) : base(xmlPath, imagePath, load)
    {
    }

    public patch_Atlas() : base(null, null, false)
    {
    }

    public Dictionary<string, Subtexture> SubTextures { get; private set; }

    public Subtexture this[string name]
    {
        [MonoModIgnore]
        get
        {
            return this.SubTextures[name];
        }
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
    internal void ctor() {}

    [MonoModIgnore]
    public extern bool Contains(string name);


    [Obsolete("Use the AtlasExt.CreateAtlas instead")]
    public static patch_Atlas Create(string xmlPath, string imagePath, bool load, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            xmlPath = Calc.LOADPATH + xmlPath;
            imagePath = Calc.LOADPATH + imagePath;
            break;
        case ContentAccess.ModContent:
            // try to access the path
            var modName = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location).Split('.');
            var modDirectory = Path.Combine("Mods", modName[0]);
            if (!Directory.Exists(modDirectory)) 
            {
                modDirectory = Path.Combine("Mods", modName[1]);
            }
            xmlPath = Path.Combine(modDirectory, "Content", xmlPath).Replace("\\", "/");
            imagePath = Path.Combine(modDirectory, "Content", imagePath).Replace("\\", "/");
            break;
        }
        XmlNodeList elementsByTagName = Calc.LoadXML(xmlPath)["TextureAtlas"].GetElementsByTagName("SubTexture");
        var atlas = new patch_Atlas() 
        {
            xmlPath = xmlPath,
            ImagePath = imagePath,
            SubTextures = new Dictionary<string, Subtexture>(elementsByTagName.Count)
            
        };
        foreach (XmlElement item in elementsByTagName)
        {
            XmlAttributeCollection attributes = item.Attributes;
            atlas.SubTextures.Add(attributes["name"].Value, new Subtexture(atlas, Convert.ToInt32(attributes["x"].Value), Convert.ToInt32(attributes["y"].Value), Convert.ToInt32(attributes["width"].Value), Convert.ToInt32(attributes["height"].Value)));
        }
        if (load)
        {
            atlas.Load();
        }
        return atlas;
    }

    internal void LoadStream(Stream fs) 
    {
        Texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
        Rect = new Rectangle(0, 0, this.Texture2D.Width, this.Texture2D.Height);
    }
}

public static class AtlasExt 
{
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

    [Obsolete("Use AtlasExt.CreateAtlas(this FortContent content, string xmlPath, string imagePath, ContentAccess access) instead.")]
    public static patch_Atlas CreateAtlas(this FortContent content, string xmlPath, string imagePath, bool load, ContentAccess access = ContentAccess.Root)
    {
        return CreateAtlas(content, xmlPath, imagePath, access);
    }

    public static patch_Atlas CreateAtlas(this FortContent content, string xmlPath, string imagePath, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content: 
            xmlPath = Calc.LOADPATH + xmlPath;
            imagePath =  Calc.LOADPATH + imagePath;
            break;
        case ContentAccess.ModContent:
            {
                if (content == null) 
                {
                    Logger.Error("[Atlas] You cannot use AtlasExt.CreateAtlas while FortContent is null");
                    return null;
                }
                using var xmlStream = content[xmlPath].Stream;
                using var imageStream = content[imagePath].Stream;
                return AtlasExt.CreateAtlas(content, xmlStream, imageStream);
            }
        }
        using var rootXmlStream = File.OpenRead(xmlPath);
        using var rootImageStream = File.OpenRead(imagePath);
        return AtlasExt.CreateAtlas(content, rootXmlStream, rootImageStream);
    }

    public static patch_Atlas CreateAtlas(this FortContent content, Stream xmlStream, Stream imageStream)
    {
        XmlNodeList elementsByTagName = patch_Calc.LoadXML(xmlStream)["TextureAtlas"].GetElementsByTagName("SubTexture");
        var atlas = new patch_Atlas();

        atlas.SetSubTextures(new Dictionary<string, Subtexture>(elementsByTagName.Count));
        foreach (XmlElement item in elementsByTagName)
        {
            XmlAttributeCollection attributes = item.Attributes;
            atlas.SubTextures.Add(
                attributes["name"].Value, 
                new Subtexture(atlas, 
                Convert.ToInt32(attributes["x"].Value), 
                Convert.ToInt32(attributes["y"].Value), 
                Convert.ToInt32(attributes["width"].Value), 
                Convert.ToInt32(attributes["height"].Value))
            );
        }

        atlas.LoadStream(imageStream);

        return atlas;
    }
}

public enum ContentAccess
{
    Root,
    Content,
    ModContent
}