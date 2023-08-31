using System;
using System.IO;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using TowerFall;
using FortRise.Adventure;
using System.Xml;

namespace FortRise;

/// <summary>
/// A class that holds the whole mod's content and provides an API to create a lookup stream.
/// </summary>
/// <remarks>
/// This class is only use for getting a mod content inside of a module. 
/// This will not interact the filesystem outside of the mod content.
/// </remarks>
public class FortContent 
{
    /// <summary>
    /// A property where your default Content lookup is being used. Some methods are relying on this.
    /// </summary>
    /// <value>The <c>ContentPath</c> property represents your Content path.</value>
    public string ContentPath 
    {
        get => contentPath;
        set => contentPath = value;
    }
    public readonly RiseCore.ModResource ResourceSystem;
    private string contentPath = "Content";
    private string modPath;
    internal string UseContent => contentPath + "/";

    public Dictionary<string, RiseCore.Resource> MapResource => ResourceSystem.Resources;
    public IReadOnlyDictionary<string, patch_Atlas> Atlases => atlases;
    private Dictionary<string, patch_Atlas> atlases = new();

    public IReadOnlyDictionary<string, patch_SFX> SFX => sfxes;
    private Dictionary<string, patch_SFX> sfxes = new();

    public IReadOnlyDictionary<string, patch_SpriteData> SpriteDatas => spriteDatas;
    private Dictionary<string, patch_SpriteData> spriteDatas = new();


    public RiseCore.Resource this[string path] 
    {
        get 
        {
            var root = $"mod:{ResourceSystem.Metadata.Name}/";
            return RiseCore.ResourceTree.TreeMap[root + path];
        }
    }

    public FortContent(ModuleMetadata metadata, RiseCore.ModResource resource) : base()
    {
        if (!string.IsNullOrEmpty(resource.Metadata.PathZip))
            modPath = resource.Metadata.PathZip;
        else
            modPath = resource.Metadata.PathDirectory;
        ResourceSystem = resource;
    }

    public FortContent(string path, RiseCore.ModResource resource) : base()
    {
        modPath = path;
        ResourceSystem = resource;
    }

    internal void Unload(bool disposeTexture) 
    {
        Dispose(disposeTexture);
    }

    internal string LoadBank(RiseCore.Resource child) 
    {
        var modDirectory = child.Root.Substring(4);
        var cachePath = "Mods/_RelinkerCache/";
        var bankDirectory = cachePath + modDirectory;
        var bankPath = bankDirectory + Path.GetFileName(child.Path);

        if (!Directory.Exists(bankDirectory))
            Directory.CreateDirectory(bankDirectory);

        var cachedChecksumPath = bankPath + ".sum";

        var stream = child.Stream;
        
        var checksums = new string[2];
        checksums[0] = RiseCore.GameChecksum;
        checksums[1] = RiseCore.GetChecksum(ref stream).ToHexadecimalString();

        if (File.Exists(bankPath) && File.Exists(cachedChecksumPath) && RiseCore.Relinker.ChecksumsEqual(
            checksums, File.ReadAllLines(cachedChecksumPath))) 
        {
            Logger.Info($"[BANK] Loading Cached Bank file to {bankPath}");
            goto LoadBankImmediately;
        }

        Logger.Info($"[BANK] Creating Bank file to {bankPath}");

        if (File.Exists(cachedChecksumPath))
            File.Delete(cachedChecksumPath);
        
        File.WriteAllLines(cachedChecksumPath, checksums);

        using (var fs = File.Create(bankPath)) 
        {
            stream.CopyTo(fs);
        }

        LoadBankImmediately:
        stream.Dispose();
        stream = null;
        return bankPath;
    }

    internal void LoadAudio() 
    {
        foreach (var child in ResourceSystem.Resources.Values.Where(x => 
            x.ResourceType == typeof(RiseCore.ResourceTypeWavFile))) 
        {
            if (child.Path.StartsWith("Content/Music")) 
            {
                var path = child.Path.Replace("Content/Music/", "");
                var extension = Path.GetExtension(child.Path);
                var audio = patch_Audio.GetMusicSystemFromExtension(extension);
                if (audio == null)
                    continue;    

                var indexOfSlash = child.Path.IndexOf('/');
                if (indexOfSlash != -1) 
                {
                    using var stream = child.Stream;
                    path = child.Root.Substring(4) + path;
                    audio.Add(path, stream);
                }
                continue;
            }
            // if (child.Path.StartsWith("Content/SFX")) 
            // {
            //     var path = child.Path.Replace("Content/SFX/", "");
            // }
        }
    }

    internal void LoadResources() 
    {
        foreach (var atlasRes in ResourceSystem.Resources.Where(x => 
            x.Value.ResourceType == typeof(RiseCore.ResourceTypeAtlas))) 
        {
            var child = atlasRes.Value;
            var png = child.Root + child.Path;
            if (Path.GetExtension(png) != ".png")
                continue;
            var xml = png.Replace(".png", ".xml");
            if (!RiseCore.ResourceTree.TreeMap.ContainsKey(xml)) 
                continue;
            int indexOfSlash = png.IndexOf('/');

            png = png.Substring(indexOfSlash + 1).Replace("Content/", "");
            xml = xml.Substring(indexOfSlash + 1).Replace("Content/", "");

            LoadAtlas(xml, png);
        }

        foreach (var spriteDataRes in ResourceSystem.Resources
            .Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeSpriteData))) 
        {
            var child = spriteDataRes.Value;
            var spriteData = child.Root + child.Path;
            if (Path.GetExtension(spriteData) != ".xml")
                continue;

            int indexOfSlash = spriteData.IndexOf('/');

            spriteData = spriteData.Substring(indexOfSlash + 1).Replace("Content/", "");

            // Will just try to load the spriteData, some spriteData might not have a neeeded attribute.
            TryLoadSpriteData(spriteData, out var result);
        }

        foreach (var gameDataRes in ResourceSystem.Resources
            .Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeGameData))) 
        {
            var child = gameDataRes.Value;
            var gameData = child.Root + child.Path;
            if (Path.GetExtension(gameData) != ".xml")
                continue;
            
            var filename = Path.GetFileName(gameData);

            switch (filename) 
            {
            case "themeData.xml": 
            {
                using var xmlStream = child.Stream;
                var xmlThemes = patch_Calc.LoadXML(xmlStream)["ThemeData"];
                foreach (XmlElement xmlTheme in xmlThemes) 
                {
                    var atlas = xmlTheme.Attr("atlas", "Atlas/atlas");
                    var themeResource = ThemeResource.Create(atlas, child);
                    var themeID = xmlTheme.Attr("id", xmlTheme.Name);
                    RiseCore.GameData.Defer(() => 
                    {
                        var towerTheme = new patch_TowerTheme(xmlTheme, child, themeResource);
                        RiseCore.GameData.Themes.Add(child.Root.Substring(4) + themeID, towerTheme);
                        Logger.Verbose("[TOWER THEME] Loaded: " + child.Root.Substring(4) + themeID);
                    }, 0);
                }
            }
                break;
            case "bgData.xml":
            {
                using var xmlStream = child.Stream;
                var xmlBGs = patch_Calc.LoadXML(xmlStream)["backgrounds"];
                foreach (XmlElement bgs in xmlBGs.GetElementsByTagName("BG")) 
                {
                    var bgID = bgs.Attr("id");
                    RiseCore.GameData.BGs.Add(child.Root.Substring(4) + bgID, bgs);
                }

                Logger.Verbose("[BG] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            case "tilesetData.xml":
            {
                using var xmlStream = child.Stream;
                var xmlTilesets = patch_Calc.LoadXML(xmlStream)["TilesetData"];
                foreach (XmlElement tilesets in xmlTilesets.GetElementsByTagName("Tileset")) 
                {
                    var atlas = tilesets.Attr("atlas", "Atlas/atlas");
                    var themeResource = ThemeResource.Create(atlas, child);
                    RiseCore.GameData.Defer(() => 
                    {
                        var tilesetID = tilesets.Attr("id", tilesets.Name);
                        RiseCore.GameData.Tilesets.Add(child.Root.Substring(4) + tilesetID, new patch_TilesetData(tilesets, themeResource));
                    }, 1);
                }

                Logger.Verbose("[Tileset] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            case "mapData.xml":
            {
                using var xmlStream = child.Stream;
                var xmlMap = patch_Calc.LoadXML(xmlStream)["map"];
                var map = new MapRendererNode(xmlMap, child);
                RiseCore.GameData.Defer(() => RiseCore.GameData.MapRenderers.Add(child.Root.Substring(4).Replace("/", ""), map), 1);
                Logger.Verbose("[MapData] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            }
        }
    }

    public bool IsResourceExist(string path) 
    {
        if (MapResource.ContainsKey(path))
            return true;
        return false;
    }

    public bool TryGetValue(string path, out RiseCore.Resource value) 
    {
        return MapResource.TryGetValue(path, out value);
    }

    public RiseCore.Resource GetValue(string path) 
    {
        return MapResource[path];
    }

    [Obsolete("Use Content.MapResources or Content[\"resourcePath\"] to look for resources")]
    public string GetContentPath(string content) 
    {
        var modDirectory = modPath.EndsWith(".dll") ? Path.GetDirectoryName(modPath) : modPath;
        return Path.Combine(modDirectory, "Content", content).Replace("\\", "/");
    }

    [Obsolete("Use Content.MapResources or Content[\"resourcePath\"] to look for resources")]
    public string GetContentPath() 
    {
        var modDirectory = modPath.EndsWith(".dll") ? Path.GetDirectoryName(modPath) : modPath;
        return Path.Combine(modDirectory, "Content").Replace("\\", "/");
    }

    public IEnumerable<string> EnumerateFilesString(string path) 
    {
        if (TryGetValue(path, out var folder)) 
        {
            for (int i = 0; i < folder.Childrens.Count; i++)
            {
                yield return folder.Childrens[i].Path;
            }
        }
    }

    public IEnumerable<RiseCore.Resource> EnumerateFiles(string path) 
    {
        if (TryGetValue(path, out var folder)) 
        {
            return folder.Childrens;
        }
        return Enumerable.Empty<RiseCore.Resource>();
    }

    public IEnumerable<RiseCore.Resource> EnumerateFiles(string path, string filterExt) 
    {
        if (TryGetValue(path, out var folder)) 
        {
            // FIXME Need improvements, this is not what I want!!!
            filterExt = filterExt.Replace("*", "");
            foreach (var child in folder.Childrens)
            {
                if (!child.Path.Contains(filterExt))
                    continue;
                yield return child;
            }
        }
    }

    public string[] GetResourcesPath(string path) 
    {
        if (TryGetValue(path, out var folder)) 
        {
            string[] childrens = new string[folder.Childrens.Count];
            for (int i = 0; i < folder.Childrens.Count; i++)
            {
                childrens[i] = folder.Childrens[i].Path;
            }
            return childrens;
        }
        return Array.Empty<string>();
    }

    public RiseCore.Resource[] GetResources(string path) 
    {
        if (TryGetValue(path, out var folder)) 
        {
            return folder.Childrens.ToArray();
        }
        return Array.Empty<RiseCore.Resource>();
    }

    [Obsolete("Use LoadAtlas(string xmlPath, string imagePath) instead")]
    public patch_Atlas LoadAtlas(string xmlPath, string imagePath, bool load = true) 
    {
        return LoadAtlas(xmlPath, imagePath);
    }

    /// <summary>
    /// Load Atlas from a mod Content path. This will use the <c>ContentPath</c> property, it's `Content/` by default.
    /// </summary>
    /// <param name="xmlPath">A path to where the xml path is.</param>
    /// <param name="imagePath">A path to where the png path is.</param>
    /// <returns>An atlas of an image.</returns>
    public patch_Atlas LoadAtlas(string xmlPath, string imagePath) 
    {
        var atlasID = xmlPath + imagePath;
        if (xmlPath.Replace(".xml", "") == imagePath.Replace(".png", "")) 
        {
            atlasID = xmlPath.Replace(".xml", "");
        }
        if (atlases.TryGetValue(atlasID, out var atlasExisted)) 
            return atlasExisted;
        
        using var xml = this[contentPath + "/" + xmlPath].Stream;
        using var image = this[contentPath + "/" + imagePath].Stream;
        var atlas = AtlasExt.CreateAtlas(this, xml, image);
        atlases.Add(atlasID, atlas);
        Logger.Verbose("[ATLAS] Loaded: " + atlasID);
        return atlas;
    }

    /// <summary>
    /// Load SpriteData from a mod Content path. 
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="filename">A path to the SpriteData xml.</param>
    /// <param name="atlas">An atlas which the spriteData will use.</param>
    /// <returns>A SpriteData instance to be use for sprite</returns>
    public patch_SpriteData LoadSpriteData(string filename, patch_Atlas atlas) 
    {
        if (spriteDatas.TryGetValue(filename, out var spriteDataExist)) 
            return spriteDataExist;
        
        using var xml = this[contentPath + "/" + filename].Stream;
        var spriteData = SpriteDataExt.CreateSpriteData(this, xml, atlas);
        spriteDatas.Add(filename, spriteData);
        return spriteData;
    }

    /// <summary>
    /// Try to load SpriteData from a mod Content path. If it succeed, it returns true if it succeed, else false.
    /// The SpriteData must have an attribute called "atlas" referencing the atlas path that exists to use to succeed.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="filename">A path to the SpriteData xml.</param>
    /// <param name="result">A SpriteData instance to be use for sprite if it succeed, else null</param>
    /// <returns>true if it succeed, else false</returns>
    public bool TryLoadSpriteData(string filename, out patch_SpriteData result)
    {
        var id = filename.Replace(".xml", "");
        if (spriteDatas.TryGetValue(id, out var spriteDataExist))
        {
            result = spriteDataExist;
            return true;
        }

        using var xml = this[contentPath + "/" + filename].Stream;
        if (!SpriteDataExt.TryCreateSpriteData(this, xml, out var data))
        {
            result = null;
            return false;
        }
        var spriteData = data;
        spriteDatas.Add(id, spriteData);
        result = spriteData;
        return true;
    }

    /// <summary>
    /// Load a stream from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="path">A path to file.</param>
    /// <returns>A FileStream or ZipStream.</returns>
    public Stream LoadStream(string path) 
    {
        return this[contentPath + "/" + path].Stream;
    }

    /// <summary>
    /// Load a raw Texture2D from an image from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <returns>A raw Texture2D that can be use for low-level texture access.</returns>
    public XNAGraphics::Texture2D LoadRawTexture2D(string path) 
    {
        using var stream = LoadStream(path);
        var tex2D = XNAGraphics::Texture2D.FromStream(Engine.Instance.GraphicsDevice, stream);
        return tex2D;
    }

    /// <summary>
    /// Load a Texture from an image from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <returns>A Monocle Texture</returns>
    public Texture LoadTexture(string path) 
    {
        var tex2D = LoadRawTexture2D(path);
        var tex = new Texture(tex2D);
        return tex;
    }

    public SFX LoadSFX(string fileName, bool obeysMasterPitch = true) 
    {
        if (sfxes.TryGetValue(fileName, out var val))
            return val;
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXExt.CreateSFX(this, stream, obeysMasterPitch);
    }

    public patch_SFXInstanced LoadSFXInstance(string fileName, int instances = 2, bool obeysMasterPitch = true) 
    {
        if (sfxes.TryGetValue(fileName, out var val))
            return (patch_SFXInstanced)val;
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXInstancedExt.CreateSFXInstanced(this, stream, instances, obeysMasterPitch);
    }

    public patch_SFXLooped LoadSFXLooped(string fileName, bool obeysMasterPitch = true) 
    {
        if (sfxes.TryGetValue(fileName, out var val))
            return (patch_SFXLooped)val;
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXLoopedExt.CreateSFXLooped(this, stream, obeysMasterPitch);
    }

    public patch_SFXVaried LoadSFXVaried(string fileName, int amount, bool obeysMasterPitch = true) 
    {
        if (sfxes.TryGetValue(fileName, out var val))
            return (patch_SFXVaried)val;
        var currentExtension = Path.GetExtension(".wav");
        fileName = fileName.Replace(currentExtension, "");
        var stream = new Stream[amount];
        for (int i = 0; i < amount; i++) 
        {
            stream[i] = this[contentPath + "/" + fileName + SFXVariedExt.GetSuffix(i + 1) + currentExtension].Stream;
        }
        return SFXVariedExt.CreateSFXVaried(this, stream, amount, obeysMasterPitch);
    }

    public void Dispose(bool disposeTexture)
    {
        ResourceSystem.Dispose();
        if (disposeTexture) 
        {
            foreach (var atlas in atlases) 
            {
                atlas.Value.Texture2D.Dispose();
            }
            atlases.Clear();
        }
    }
}