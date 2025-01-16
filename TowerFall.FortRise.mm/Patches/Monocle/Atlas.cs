using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FortRise;
using FortRise.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;

namespace Monocle;

public class patch_Atlas : Atlas
{
    private string xmlPath;
    internal HashSet<string> injectedAtlas = new HashSet<string>();

    public patch_Atlas(string xmlPath, string imagePath, bool load) : base(xmlPath, imagePath, load) {}

    public patch_Atlas() : base(null, null, false) {}

    public extern void orig_ctor(string xmlPath, string imagePath, bool load);

    [MonoModConstructor]
    public void ctor(string xmlPath, string imagePath, bool load) 
    {
        injectedAtlas = new HashSet<string>();
        orig_ctor(xmlPath, imagePath, load);

        TaggedSubTextures = new();
    }


    public static void MergeAtlas(RiseCore.Resource resource, patch_Atlas source, Atlas destination, string prefix) 
    {
        foreach (var subTexture in source.SubTextures) 
        {
            destination.SubTextures.Add(prefix + subTexture.Key, subTexture.Value);
        }

        destination.GetAllInjectedAtlas().Add(resource.Root + resource.Path);
    }

    public static void MergeTexture(RiseCore.Resource resource, Subtexture source, Atlas destination, string prefix) 
    {
        var pngPath = resource.RootPath;
        int indexOfSlash = pngPath.IndexOf('/');
        var key = Path.ChangeExtension(pngPath.Substring(indexOfSlash + 1)
            .Replace("Content/Atlas/atlas/", "")
            .Replace("Content/Atlas/menuAtlas/", "")
            .Replace("Content/Atlas/bossAtlas/", "")
            .Replace("Content/Atlas/bgAtlas", ""), null);
        
        var filename = Path.GetFileName(key);

        if (filename.StartsWith("(")) 
        {
            int index = filename.IndexOf('(');
            int ending = key.IndexOf(')');
            var actualKey = key.Substring(0, ending);

            var tagPath = Path.ChangeExtension(pngPath, "tag");
            if (ModIO.IsFileExists(tagPath)) 
            {
                patch_Atlas dest = (patch_Atlas)destination;               
                using var text = ModIO.OpenText(tagPath);
                var tags = Calc.ReadCSV(text.ReadLine());

                foreach (var tag in tags) 
                {
                    if (dest.TaggedSubTextures.TryGetValue(tag, out var textures)) 
                    {
                        Logger.Log(actualKey);
                        textures.Add(actualKey.Replace("(", ""), source);
                    }
                    else 
                    {
                        var newTextures = new Dictionary<string, Subtexture>();
                        newTextures.Add(actualKey.Replace("(", ""), source);
                        dest.TaggedSubTextures.Add(tag, newTextures);
                    }
                }
            }
            else 
            {
                destination.SubTextures[actualKey.Replace("(", "")] = source;
            }
        }
        else 
        {
            destination.SubTextures[prefix + key] = source;
        }

        destination.GetAllInjectedAtlas().Add(pngPath);
    }

    public Dictionary<string, Subtexture> SubTextures { get; private set; }
    public Dictionary<string, Dictionary<string, Subtexture>> TaggedSubTextures { get; private set; }

    public Subtexture this[string name]
    {
        [MonoModReplace]
        get
        {
            if (TaggedSubTextures == null)
                return this.SubTextures[name];

            var scene = Engine.Instance.Scene;

            if (scene?.GetSceneTags() != null) 
            {
                foreach (var tag in scene.GetSceneTags())
                {
                    if (TaggedSubTextures.TryGetValue(tag, out var tex))
                    {
                        if (tex.TryGetValue(name, out var val))
                            return val;
                    }
                }
            }

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

    public static patch_Atlas CreateAtlas(FortContent content, string xmlPath, string imagePath, ContentAccess access = ContentAccess.Root)
    {
        if (access == ContentAccess.Content) 
        {
            xmlPath = Calc.LOADPATH + xmlPath;
            imagePath =  Calc.LOADPATH + imagePath;
        }
        else if (content != null) 
        {
            xmlPath = Path.Combine(content.MetadataPath, xmlPath);
            imagePath = Path.Combine(content.MetadataPath, imagePath);
        }
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
