using System.Collections.Generic;
using System.Xml;
using MonoMod;

namespace Monocle;

public class patch_SpriteData 
{

    public patch_SpriteData() {} 

    [MonoModConstructor]
    internal void ctor() {}

    private Atlas atlas;

    private Dictionary<string, XmlElement> sprites;

    public static patch_SpriteData Create(string filename, Atlas atlas)
    {
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