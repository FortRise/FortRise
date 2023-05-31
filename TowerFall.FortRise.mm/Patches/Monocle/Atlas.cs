using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using FortRise;
using MonoMod;

namespace Monocle;

public class patch_Atlas : Texture
{
    private string xmlPath;
    public Dictionary<string, Subtexture> SubTextures { get; private set; }

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


    
    public patch_Atlas() {} 

    [MonoModConstructor]
    internal void ctor() {}


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
}

public static class AtlasExt 
{
    public static patch_Atlas CreateAtlas(this FortContent content, string xmlPath, string imagePath, bool load, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            xmlPath = Calc.LOADPATH + xmlPath;
            imagePath = Calc.LOADPATH + imagePath;
            break;
        case ContentAccess.ModContent:
            xmlPath = content.GetContentPath(xmlPath);
            imagePath = content.GetContentPath(imagePath);
            break;
        }
        XmlNodeList elementsByTagName = Calc.LoadXML(xmlPath)["TextureAtlas"].GetElementsByTagName("SubTexture");
        var atlas = new patch_Atlas();

        atlas.SetXMLPath(xmlPath);
        atlas.SetImagePath(imagePath);
        atlas.SetSubTextures(new Dictionary<string, Subtexture>(elementsByTagName.Count));
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
}

public enum ContentAccess
{
    Root,
    Content,
    ModContent
}