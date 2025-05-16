using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
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
    public static Dictionary<string, XmlElement> GetSprites(this SpriteData spriteData)
    {
        return ((patch_SpriteData)spriteData).GetSprites();
    }
}