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


    
    public patch_Atlas() {} 

    [MonoModConstructor]
    internal void ctor() {}

    public static patch_Atlas Create(string xmlPath, string imagePath, bool load, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            xmlPath = Calc.LOADPATH + xmlPath;
            imagePath = Calc.LOADPATH + imagePath;
            break;
        case ContentAccess.ModContent:
            var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
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

public enum ContentAccess
{
    Root,
    Content,
    ModContent
}