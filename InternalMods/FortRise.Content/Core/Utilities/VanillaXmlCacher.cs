using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Monocle;

namespace FortRise.Content;

internal static class VanillaXmlCacher 
{
    private static HashSet<string>? atlasIDCached;
    private static Dictionary<string, XmlDocument> xmls = new();
    

    public static XmlDocument? LoadXml(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            return null; 
        }

        ref var col = ref CollectionsMarshal.GetValueRefOrAddDefault(xmls, xmlPath, out bool exists);
        if (exists)
        {
            return col;
        }

        return Calc.LoadXML(xmlPath);
    }

    public static bool CheckDarkWorld()
    {
        return File.Exists("DarkWorldContent/Atlas/atlas.png");
    }

    public static HashSet<string> GetAllAvailableSubtexturesID() 
    {
        if (atlasIDCached is not null)
        {
            return atlasIDCached;
        }
        atlasIDCached = new();
        string atlasPath;
        string bossAtlasPath;

        if (CheckDarkWorld()) 
        {
            atlasPath = "DarkWorldContent/Atlas/atlas.xml";
            bossAtlasPath = "DarkWorldContent/Atlas/bossAtlas.xml";
        }
        else 
        {
            atlasPath = "Content/Atlas/atlas.xml";
            bossAtlasPath = "Content/Atlas/bossAtlas.xml";
        }

        string menuAtlasPath = "Content/Atlas/menuAtlas.xml";
        string bgAtlasPath = "Content/Atlas/bgAtlas.xml";

        var atlasDoc = LoadXml(atlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(bgAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(bossAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        atlasDoc = LoadXml(menuAtlasPath)!["TextureAtlas"];
        foreach (XmlElement elm in atlasDoc!.GetElementsByTagName("SubTexture"))
        {
            atlasIDCached.Add(elm.Attr("name"));
        }

        return atlasIDCached;
    }
}
