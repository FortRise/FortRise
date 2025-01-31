using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using FortRise;
using FortRise.IO;
using MonoMod;

namespace Monocle;

public class patch_SpriteData : SpriteData
{
    public patch_SpriteData() :base(null, null) {} 

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

    internal Dictionary<string, XmlElement> GetSprites() 
    {
        return sprites;
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
        {
            if (content == null) 
            {
                Logger.Error("[SpriteData] You cannot use SpriteDataExt.CreateSpriteData while FortContent is null");
                return null;
            }
            using var fileStream = content[filename].Stream;
            return SpriteDataExt.CreateSpriteData(fileStream, atlas);
        }
        }
        using var stream = File.OpenRead(filename);
        return SpriteDataExt.CreateSpriteData(stream, atlas);
    }

    public static patch_SpriteData CreateSpriteData(string filename, patch_Atlas atlas) 
    {
        using var stream = ModIO.OpenRead(filename);
        return CreateSpriteData(stream, atlas);
    }

    public static patch_SpriteData CreateSpriteData(Stream stream, patch_Atlas atlas)
    {
        XmlDocument xmlDocument = patch_Calc.LoadXML(stream);
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

    public static patch_SpriteData CreateSpriteDataFromAtlas(this FortContent content, Stream filename, patch_Atlas atlas)
    {
        var xmlElement = patch_Calc.LoadXML(filename)["SpriteData"];
        var sprites = new Dictionary<string, XmlElement>();
        
        foreach (object item in xmlElement)
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

    public static bool TryCreateSpriteData(this FortContent content, Stream filename, out patch_SpriteData data)
    {
        var xmlElement = patch_Calc.LoadXML(filename)["SpriteData"];
        var sprites = new Dictionary<string, XmlElement>();
        var attr = xmlElement.Attr("atlas", "Atlas/atlas");
        if (!content.Atlases.TryGetValue(attr, out var atlas)) 
        {
            data = null;
            return false;
        }
        foreach (object item in xmlElement)
        {
            if (item is XmlElement)
            {
                sprites.Add((item as XmlElement).Attr("id"), item as XmlElement);
            }
        }
        var spriteData = new patch_SpriteData();
        
        spriteData.SetAtlasAndSprite(atlas, sprites);
        data = spriteData;
        return true;
    }

    public static Dictionary<string, XmlElement> GetSprites(this SpriteData spriteData)
    {
        return ((patch_SpriteData)spriteData).GetSprites();
    }
}