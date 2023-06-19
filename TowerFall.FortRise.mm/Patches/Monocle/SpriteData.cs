using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using FortRise;
using MonoMod;

namespace Monocle;

public class patch_SpriteData 
{
    public patch_SpriteData() {} 

    [MonoModConstructor]
    internal void ctor() {}

    private patch_Atlas atlas;
    private Dictionary<string, XmlElement> sprites;

    [MonoModIgnore]
    public extern Sprite<int> GetSpriteInt(string id);

    [MonoModIgnore]
    public extern Sprite<string> GetSpriteString(string id);

    internal void SetAtlasAndSprite(patch_Atlas atlas, Dictionary<string, XmlElement> sprites) 
    {
        this.atlas = atlas;
        this.sprites = sprites;
    }

    [Obsolete("Use SpriteDataExt.CreateSpriteData instead")]
    public static patch_SpriteData Create(string filename, patch_Atlas atlas, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            filename = Calc.LOADPATH + filename;
            break;
        case ContentAccess.ModContent:
            // try to access the path
            var modName = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location).Split('.');
            var modDirectory = Path.Combine("Mods", modName[0]);
            if (!Directory.Exists(modDirectory)) 
            {
                modDirectory = Path.Combine("Mods", modName[1]);
            }
            filename = Path.Combine(modDirectory, "Content", filename).Replace("\\", "/");
            break;
        }
        XmlDocument xmlDocument = Calc.LoadXML(filename);
        var sprites = new Dictionary<string, XmlElement>();
        foreach (object item in xmlDocument["SpriteData"])
        {
            if (item is XmlElement)
            {
                sprites.Add((item as XmlElement).Attr("id"), item as XmlElement);
            }
        }
        var spriteData = new patch_SpriteData() 
        {
            atlas = atlas,
            sprites = sprites
        };
        return spriteData;
    }
}

public static class SpriteDataExt 
{
    public static patch_SpriteData CreateSpriteData(this FortContent content, string filename, patch_Atlas atlas, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            filename = Calc.LOADPATH + filename;
            break;
        case ContentAccess.ModContent:
            filename = content.GetContentPath(filename);
            break;
        }
        XmlDocument xmlDocument = Calc.LoadXML(filename);
        var sprites = new Dictionary<string, XmlElement>();
        foreach (object item in xmlDocument["SpriteData"])
        {
            if (item is XmlElement)
            {
                sprites.Add((item as XmlElement).Attr("id"), item as XmlElement);
            }
        }
        var spriteData = new patch_SpriteData();

        spriteData.SetAtlasAndSprite(atlas, sprites);
        return spriteData;
    }
}