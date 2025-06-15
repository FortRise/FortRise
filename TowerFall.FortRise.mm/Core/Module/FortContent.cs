using System;
using System.IO;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using TowerFall;
using System.Xml;
using System.Text.Json;
using Mono.Cecil;

namespace FortRise;

/// <summary>
/// A class that holds the whole mod's content and provides an API to create a lookup stream.
/// </summary>
/// <remarks>
/// This class is only use for getting a mod content inside of a module.
/// This will not interact the filesystem outside of the mod content.
/// </remarks>
public class FortContent : IModContent
{
    public delegate void DataLoaderHandler(IResourceInfo resource, string rootPath);
    public static event DataLoaderHandler DataLoader;
    /// <summary>
    /// A property where your default Content lookup is being used. Some methods are relying on this.
    /// </summary>
    /// <value>The <c>ContentPath</c> property represents your Content path.</value>
    public string ContentPath => "Content";
    
    public readonly IModResource ResourceSystem;

    public Dictionary<string, IResourceInfo> MapResource => ResourceSystem.OwnedResources;

    private FileSystemWatcher watcher;
    private Dictionary<string, WatchTexture> watchingTexture = new Dictionary<string, WatchTexture>();
    private string requestedPath;
    private bool requestReload;

    [Obsolete]
    public string MetadataPath => Root.Root;
    
    /// <summary>
    /// The mod's Content root path.
    /// </summary>
    /// <value>Represent the prefix of your path to the virtual filesystem</value>
    public IResourceInfo Root
    {
        get
        {
            if (ResourceSystem.Metadata == null)
            {
                return RiseCore.ResourceTree.Get("mod:::global:/");
            }
            return RiseCore.ResourceTree.Get($"mod:{ResourceSystem.Metadata.Name}/Content");
        }
    }

    public ModuleMetadata Metadata { get; init; }

    [Obsolete("Use the IResourceInfo.Root.GetRelativePath() instead")]
    public IResourceInfo this[string path]
    {
        get
        {
            path = path.Replace('\\', '/');
            var root = $"mod:{ResourceSystem.Metadata.Name}/";
            return RiseCore.ResourceTree.TreeMap[root + path];
        }
    }

    public FortContent(IModResource resource, ModuleMetadata metadata)
    {
        ResourceSystem = resource;
        Metadata = metadata;
    }

    private void OnReload(object sender, FileSystemEventArgs e)
    {
        if (requestReload) // prevents another call
        {
            return;
        }
        requestReload = true;
        RiseCore.ResourceReloader.ContentRequestedReload.Enqueue(this);
        requestedPath = e.FullPath;
    }

    internal void Unload(bool disposeTexture)
    {
        Dispose(disposeTexture);
    }

    internal void Reload() 
    {
        requestReload = false;

        if (!watchingTexture.TryGetValue(requestedPath.Replace('\\', '/'), out var watchTexture))
        {
            return;
        }

        if (watchTexture.Type == typeof(RiseCore.ResourceTypeAtlasPng) || 
            watchTexture.Type == typeof(RiseCore.ResourceTypeBGAtlasPng) ||
            watchTexture.Type == typeof(RiseCore.ResourceTypeBossAtlasPng) ||
            watchTexture.Type == typeof(RiseCore.ResourceTypeMenuAtlasPng) ||
            watchTexture.Type == typeof(RiseCore.ResourceTypeAtlas))
        {
            try 
            {
                using var resource = ModIO.OpenRead(requestedPath.Replace('\\', '/'));
                using CPUImage image = new CPUImage(resource);
                watchTexture.Texture.Texture2D.SetData(image.Pixels.ToArray());
            }
            catch (FailedToLoadImageException)
            {
                Logger.Error("Failed to load image error, try again!");
            }
        }
    }

    internal void LoadAudio()
    {
        foreach (var child in ResourceSystem.OwnedResources.Values.Where(x =>
            x.ResourceType == typeof(RiseCore.ResourceTypeWavFile) || x.ResourceType == typeof(RiseCore.ResourceTypeOggFile)))
        {
            if (child.Path.StartsWith("Content/Music"))
            {
                var root = child.Resource.Metadata.Name + "/";
                var path = child.Path.Replace("Content/Music/", "");
                var id = root + path;

                var indexOfSlash = child.Path.IndexOf('/');
                if (indexOfSlash != -1)
                {
                    var trackInfo = new TrackInfo(id, child);
                    patch_Audio.TrackMap[id] = trackInfo;
                    Logger.Verbose($"[MUSIC] [{child.Resource.Metadata.Name}] Added '{id}' to TrackMap.");
                }
                continue;
            }
        }

        foreach (var soundRes in ResourceSystem.OwnedResources.Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeXMLSoundBank)))
        {
            var child = soundRes.Value;

            XmlElement soundBank = child.Xml["SoundBank"];

            foreach (XmlElement sfx in soundBank)
            {
                var id = sfx.Attr("id");
                var path = Path.Combine(Path.GetDirectoryName(child.Path), id).Replace('\\', '/');
                var obeysMasterPitch = sfx.AttrBool("obeyMasterPitch", true);

                var checkPath = Path.Combine(child.Root, path);

                if (!ModIO.IsFileExists(checkPath + ".wav"))
                {
                    if (ModIO.IsFileExists(checkPath + ".ogg"))
                    {
                        path += ".ogg";
                    }
                    else 
                    {
                        Logger.Error($"This specific id: '{id}' cannot be mapped into any audio file. (.WAV or .OGG)");
                        continue;
                    }
                }
                else 
                {
                    path += ".wav";
                }

                path = path.Replace("Content/", "");

                switch (sfx.Name)
                {
                case "SFX":
                    var createdSFX = LoadSFX(path, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFX);
                    break;
                case "SFXInstanced":
                    var instances = sfx.AttrInt("instances", 2);
                    var createdSFXInstanced = LoadSFXInstance(path, instances, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXInstanced);
                    break;
                case "SFXVaried":
                    var amount = sfx.AttrInt("count");
                    var createdSFXVaried = LoadSFXVaried(path, amount, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXVaried);
                    break;
                case "SFXLooped":
                    var createdSFXLooped = LoadSFXLooped(path, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXLooped);
                    break;
                }
            }
        }

        foreach (var soundRes in ResourceSystem.OwnedResources.Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeJSONSoundBank)))
        {
            var child = soundRes.Value;

            using var childStream = child.Stream;

            var dict = JsonSerializer.Deserialize<Dictionary<string, SFXSoundBank>>(childStream);

            foreach (var pair in dict)
            {
                var id = pair.Key;
                var path = Path.Combine(Path.GetDirectoryName(child.Path), id).Replace('\\', '/');
                var obeysMasterPitch = pair.Value.ObeysMasterPitch;

                var checkPath = Path.Combine(child.Root, path);

                if (!ModIO.IsFileExists(checkPath + ".wav"))
                {
                    if (ModIO.IsFileExists(checkPath + ".ogg"))
                    {
                        path += ".ogg";
                    }
                    else 
                    {
                        Logger.Error($"This specific id: '{id}' cannot be mapped into any audio file. (.WAV or .OGG)");
                        continue;
                    }
                }
                else 
                {
                    path += ".wav";
                }

                path = path.Replace("Content/", "");
                var type = pair.Value.Type.ToLowerInvariant();

                switch (pair.Value.Type)
                {
                case "sfx":
                    var createdSFX = LoadSFX(path, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFX);
                    break;
                case "instanced":
                    var instances = pair.Value.Instances;
                    var createdSFXInstanced = LoadSFXInstance(path, instances, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXInstanced);
                    break;
                case "varied":
                    var amount = pair.Value.Count;
                    var createdSFXVaried = LoadSFXVaried(path, amount, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXVaried);
                    break;
                case "looped":
                    var createdSFXLooped = LoadSFXLooped(path, obeysMasterPitch);
                    patch_Sounds.AddSFX(this, id, createdSFXLooped);
                    break;
                }
            }
        }
    }

    internal void LoadResources() 
    {
        // Loads all atlas that has been crawled, this should only look for .png and .xml
        foreach (var atlasRes in ResourceSystem.OwnedResources.Where(x =>
            x.Value.ResourceType == typeof(RiseCore.ResourceTypeAtlas)))
        {
            var child = atlasRes.Value;
            var png = child.RootPath;
            if (Path.GetExtension(png) != ".png") 
            {
                continue;
            }

            string data = null;
            foreach (var ext in AtlasReader.InternalReaders.Keys)
            {
                data = png.Replace(".png", ext);
                if (RiseCore.ResourceTree.TreeMap.ContainsKey(data))
                {
                    break;
                }
            }

            int indexOfSlash = png.IndexOf('/');

            png = png.Substring(indexOfSlash + 1).Replace("Content/", "");
            data = data.Substring(indexOfSlash + 1).Replace("Content/", "");

            var atlas = LoadAtlas(data, png);

            if (ResourceSystem.Metadata.IsDirectory)
            {
                // might be bad, but all we care about is the Texture2D to change data anyway.
                watchingTexture[child.FullPath] = new WatchTexture(child.ResourceType, new Subtexture(new Texture(atlas.Texture2D)));
            }

            string filename = Path.GetFileNameWithoutExtension(png);

            if (filename == "menuAtlas") 
            {
                patch_Atlas.MergeAtlas(child, atlas, TFGame.MenuAtlas, child.Root.Substring(4));
            }
            else if (filename == "bossAtlas") 
            {
                patch_Atlas.MergeAtlas(child, atlas, TFGame.BossAtlas, child.Root.Substring(4));
            }
            else if (filename == "bgAtlas") 
            {
                patch_Atlas.MergeAtlas(child, atlas, TFGame.BGAtlas, child.Root.Substring(4));
            }
            else 
            {
                patch_Atlas.MergeAtlas(child, atlas, TFGame.Atlas, child.Root.Substring(4));
            }
        }

        foreach (var atlasPng in ResourceSystem.OwnedResources
            .Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeAtlasPng) || 
                        x.Value.ResourceType == typeof(RiseCore.ResourceTypeBGAtlasPng) ||
                        x.Value.ResourceType == typeof(RiseCore.ResourceTypeBossAtlasPng) ||
                        x.Value.ResourceType == typeof(RiseCore.ResourceTypeMenuAtlasPng))) 
        {
            var child = atlasPng.Value;
            using var stream = child.Stream;
            var texture = XNAGraphics.Texture2D.FromStream(TFGame.Instance.GraphicsDevice, stream);
            var subtexture = new Subtexture(new Monocle.Texture(texture));

            watchingTexture[child.FullPath] = new WatchTexture(child.ResourceType, subtexture);

            if (child.ResourceType == typeof(RiseCore.ResourceTypeAtlasPng)) 
            {
                patch_Atlas.MergeTexture(child, subtexture, TFGame.Atlas, child.Root.Substring(4));
            }
            else if (child.ResourceType == typeof(RiseCore.ResourceTypeMenuAtlasPng)) 
            {
                patch_Atlas.MergeTexture(child, subtexture, TFGame.MenuAtlas, child.Root.Substring(4));
            }
            else if (child.ResourceType == typeof(RiseCore.ResourceTypeBGAtlasPng)) 
            {
                patch_Atlas.MergeTexture(child, subtexture, TFGame.BGAtlas, child.Root.Substring(4));
            }
            else if (child.ResourceType == typeof(RiseCore.ResourceTypeBossAtlasPng)) 
            {
                patch_Atlas.MergeTexture(child, subtexture, TFGame.BossAtlas, child.Root.Substring(4));
            }
        }

        foreach (var spriteDataRes in ResourceSystem.OwnedResources
            .Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeSpriteData)))
        {
            var child = spriteDataRes.Value;
            var filename = Path.GetFileName(child.Path);

            var spriteDataXML = ModIO.LoadXml(child)["SpriteData"];

            foreach (object elm in spriteDataXML)
            {
                if (elm is not XmlElement item)
                {
                    continue;
                }
                if (filename == "spriteData.xml")
                {
                    TFGame.SpriteData.GetSprites().Add(child.Root.Substring(4) + item.Attr("id"), item);
                }
                else if (filename == "menuSpriteData.xml")
                {
                    TFGame.MenuSpriteData.GetSprites().Add(child.Root.Substring(4) + item.Attr("id"), item);
                }
                else if (filename == "corpseSpriteData.xml")
                {
                    TFGame.CorpseSpriteData.GetSprites().Add(child.Root.Substring(4) + item.Attr("id"), item);
                }
                else if (filename == "bossSpriteData.xml")
                {
                    TFGame.BossSpriteData.GetSprites().Add(child.Root.Substring(4) + item.Attr("id"), item);
                }
                else if (filename == "bgSpriteData.xml")
                {
                    TFGame.BGSpriteData.GetSprites().Add(child.Root.Substring(4) + item.Attr("id"), item);
                }
            }
        }

        foreach (var gameDataRes in ResourceSystem.OwnedResources
            .Where(x => x.Value.ResourceType == typeof(RiseCore.ResourceTypeGameData)))
        {
            var child = gameDataRes.Value;
            var gameData = child.RootPath;
            if (Path.GetExtension(gameData) != ".xml") 
            {
                continue;
            }

            var filename = Path.GetFileName(gameData);

            switch (filename)
            {
            case "bgData.xml":
            {
                var xmlBGs = ModIO.LoadXml(child)["backgrounds"];
                foreach (XmlElement bgs in xmlBGs.GetElementsByTagName("BG"))
                {
                    var bgID = bgs.Attr("id");
                    ExtendedGameData.Defer(() =>
                    {
                        TowerFall.GameData.BGs[child.Root.Substring(4) + bgID] = bgs;
                    }, 1);
                }

                Logger.Verbose("[BG] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            case "tilesetData.xml":
            {
                var xmlTilesets = ModIO.LoadXml(child)["TilesetData"];
                foreach (XmlElement tilesets in xmlTilesets.GetElementsByTagName("Tileset"))
                {
                    ExtendedGameData.Defer(() =>
                    {
                        var tilesetID = tilesets.Attr("id", tilesets.Name);
                        TowerFall.GameData.Tilesets[child.Root.Substring(4) + tilesetID] = new patch_TilesetData(tilesets);
                    }, 1);
                }

                Logger.Verbose("[Tileset] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            case "mapData.xml":
            {
                var xmlMap = ModIO.LoadXml(child)["map"];
                var map = new MapRendererNode(xmlMap);
                ExtendedGameData.Defer(() => ExtendedGameData.InternalMapRenderers.Add(child.Root.Substring(4).Replace("/", ""), map), 1);
                Logger.Verbose("[MapData] Loaded: " + child.Root.Substring(4) + child.Path);
            }
                break;
            default:
                DataLoader?.Invoke(child, gameData);
                break;
            }
        }

        if (ResourceSystem.Metadata != null && ResourceSystem.Metadata.IsDirectory) 
        {
            var dir = Path.Combine(ResourceSystem.Metadata.PathDirectory, ContentPath);
            if (!Directory.Exists(dir)) 
            {
                return;
            }
            watcher = new FileSystemWatcher(dir);
            watcher.EnableRaisingEvents = true;
            watcher.Changed += OnReload;
            watcher.IncludeSubdirectories = true;
        }
    }

    public T LoadShader<T>(string path, string passName, out int id)
    where T : ShaderResource, new()
    {
        path = path.Replace('\\', '/');
        return ShaderManager.AddShader<T>(Root.GetRelativePath(path), passName, out id);
    }

    public TrackInfo LoadMusic(string path)
    {
        path = path.Replace('\\', '/');
        var musicPath = ContentPath + "/" + path;
        var musicResource = Root.GetRelativePath(path);
        var trackInfo = new TrackInfo(path, musicResource);
        return trackInfo;
    }

    /// <summary>
    /// Load Atlas from a mod Content path. This will use the <c>ContentPath</c> property, it's `Content/` by default.
    /// </summary>
    /// <param name="dataPath">A path to where the xml or json path is.</param>
    /// <param name="imagePath">A path to where the png path is.</param>
    /// <returns>An atlas of an image.</returns>
    public patch_Atlas LoadAtlas(string dataPath, string imagePath)
    {
        var ext = Path.GetExtension(dataPath);
        var atlasID = dataPath + imagePath;
        if (dataPath.Replace(ext, "") == imagePath.Replace(".png", ""))
        {
            atlasID = dataPath.Replace(ext, "");
        }

        using var data = Root.GetRelativePath(dataPath).Stream;
        using var image = Root.GetRelativePath(imagePath).Stream;

        var atlas = AtlasExt.CreateAtlas(data, image, ext);
        Logger.Verbose("[ATLAS] Loaded: " + atlasID);
        return atlas;
    }

    /// <summary>
    /// Load Atlas from a mod Content path without extension. This will use the <c>ContentPath</c> property, it's `Content/` by default.
    /// </summary>
    /// <param name="path">A path to where an atlas data name and atlas png name can be match together</param>
    /// <returns>An atlas of an image.</returns>
    public patch_Atlas LoadAtlas(string path)
    {
        path = path.Replace('\\', '/');
        var ext = Path.GetExtension(path);
        var atlasID = path;
        if (!string.IsNullOrEmpty(ext))
        {
            var idx = path.IndexOf(".");
            path = path.Substring(0, idx);
        }

        IResourceInfo dataRes;
        if (!Root.TryGetRelativePath(path + ".xml", out dataRes))
        {
            if (!Root.TryGetRelativePath(path + ".json", out dataRes))
            {
                Logger.Error($"[ATLAS] No data xml or json found in this path {path}");
                return null;
            }
        }

        using var data = dataRes.Stream;
        using var image = Root.GetRelativePath(path + ".ong").Stream;
        var atlas = AtlasExt.CreateAtlas(data, image, ext);
        Logger.Verbose("[ATLAS] Loaded: " + atlasID);
        return atlas;
    }

    /// <summary>
    /// Load SpriteData from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="path">A path to the SpriteData xml.</param>
    /// <param name="atlas">An atlas which the spriteData will use.</param>
    /// <returns>A SpriteData instance to be use for sprite</returns>
    public patch_SpriteData LoadSpriteData(string path, patch_Atlas atlas)
    {
        using var xml = Root.GetRelativePath(path).Stream;
        XmlDocument xmlDocument = patch_Calc.LoadXML(xml);
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

    /// <summary>
    /// Load a stream from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="path">A path to file.</param>
    /// <returns>A FileStream or ZipStream.</returns>
    [Obsolete("Use Root.GetRelativePath(path).Stream instead")]
    public Stream LoadStream(string path)
    {
        path = path.Replace('\\', '/');
        return Root.GetRelativePath(path).Stream;
    }

    /// <summary>
    /// Load a raw Texture2D from an image from a mod Content path.
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <returns>A raw Texture2D that can be use for low-level texture access.</returns>
    public XNAGraphics::Texture2D LoadRawTexture2D(string path)
    {
        path = path.Replace('\\', '/');
        using var stream = Root.GetRelativePath(path).Stream;
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
        path = path.Replace('\\', '/');
        var tex2D = LoadRawTexture2D(path);
        var tex = new Texture(tex2D);
        return tex;
    }

    /// <summary>
    /// Load text inside of a file
    /// </summary>
    /// <param name="path">A path to a file</param>
    /// <returns>A text inside of a file</returns>
    [Obsolete("Use Root.GetRelativePath(path).Text instead")]
    public string LoadText(string path)
    {
        return Root.GetRelativePath(path).Text;
    }

    /// <summary>
    /// Load <see cref="System.Xml.XmlDocument"/> from a file.
    /// </summary>
    /// <param name="path">A path to a file</param>
    /// <returns>A <see cref="System.Xml.XmlDocument"/></returns>
    [Obsolete("Use Root.GetRelativePath(path).Xml instead")]
    public XmlDocument LoadXML(string path)
    {
        return Root.GetRelativePath(path).Xml;
    }

    public SFX LoadSFX(string path, bool obeysMasterPitch = true)
    {
        using var stream = Root.GetRelativePath(path).Stream;
        return SFXExt.CreateSFX(this, stream, obeysMasterPitch);
    }

    public patch_SFXInstanced LoadSFXInstance(string path, int instances = 2, bool obeysMasterPitch = true)
    {
        using var stream = Root.GetRelativePath(path).Stream;
        return SFXInstancedExt.CreateSFXInstanced(this, stream, instances, obeysMasterPitch);
    }

    public patch_SFXLooped LoadSFXLooped(string path, bool obeysMasterPitch = true)
    {
        using var stream = Root.GetRelativePath(path).Stream;
        return SFXLoopedExt.CreateSFXLooped(this, stream, obeysMasterPitch);
    }

    public patch_SFXVaried LoadSFXVaried(string path, int amount, bool obeysMasterPitch = true)
    {
        var currentExtension = Path.GetExtension(".wav");
        path = path.Replace(currentExtension, "");
        var stream = new Stream[amount];
        for (int i = 0; i < amount; i++)
        {
            stream[i] = Root.GetRelativePath(path + SFXVariedExt.GetSuffix(i + 1) + currentExtension).Stream;
        }
        return SFXVariedExt.CreateSFXVaried(this, stream, amount, obeysMasterPitch);
    }

    internal void Dispose(bool disposeTexture)
    {
        // ResourceSystem.Dispose();
    }

    public ISubtextureEntry LoadTexture(IResourceInfo file)
    {
        throw new NotImplementedException();
    }

    public ISubtextureEntry LoadTexture(Func<Subtexture> callback)
    {
        throw new NotImplementedException();
    }

    private struct WatchTexture(Type type, Subtexture texture)
    {
        public Type Type = type;
        public Subtexture Texture = texture;
    }
}

public enum ContentAccess
{
    Root,
    Content,
    ModContent
}
