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

    private Atlas atlas;

    private Dictionary<string, XmlElement> sprites;

    public static patch_SpriteData Create(string filename, Atlas atlas, ContentAccess access = ContentAccess.Root)
    {
        switch (access) 
        {
        case ContentAccess.Content:
            filename = Calc.LOADPATH + filename;
            break;
        case ContentAccess.ModContent:
            var modDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            filename = Path.Combine(modDirectory, "Content", filename).Replace("\\", "/");
            break;
        }
        filename = access switch 
        {
            ContentAccess.Content => Calc.LOADPATH + filename,
            ContentAccess.ModContent => PathUtils.ToContentPath(filename),
            _ => filename
        };
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