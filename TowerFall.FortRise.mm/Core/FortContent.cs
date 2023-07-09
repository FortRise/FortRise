using System;
using System.IO;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using Ionic.Zip;
using System.Linq;

namespace FortRise;

/// <summary>
/// A class that holds the whole mod's content and provides an API to create a lookup stream.
/// </summary>
/// <remarks>
/// This class is only use for getting a mod content inside of a module. 
/// This will not interact the filesystem outside of the mod content.
/// </remarks>
public class FortContent : IDisposable
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
    public readonly RiseCore.ResourceSystem ResourceSystem;
    private string contentPath = "Content";
    private string modPath;
    internal string UseContent => contentPath + "/";

    public Dictionary<string, RiseCore.Resource> MapResource => ResourceSystem.MapResource;
    public IReadOnlyDictionary<string, patch_Atlas> Atlases => atlases;
    private Dictionary<string, patch_Atlas> atlases = new();

    public IReadOnlyDictionary<string, patch_SpriteData> SpriteDatas => spriteDatas;
    private Dictionary<string, patch_SpriteData> spriteDatas = new();

    public RiseCore.Resource this[string path] => ResourceSystem.MapResource[path];

    public FortContent(FortModule module) : base()
    {
        modPath = module.Meta.DLL;
    }

    public FortContent(string dir, bool isZip = false) : base()
    {
        modPath = dir;
        if (isZip) 
        {
            var zipFile = ZipFile.Read(dir);
            ResourceSystem = new RiseCore.ZipResourceSystem(zipFile);
        }
        else
            ResourceSystem = new RiseCore.FolderResourceSystem(dir);
        ResourceSystem.Open(dir);
    }

    internal void Unload() 
    {
        Dispose();
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

    public RiseCore.Resource GetResource(string path) 
    {
        return MapResource[path];
    }

    [Obsolete("Use Content.MapResources to look for resources")]
    public string GetContentPath(string content) 
    {
        var modDirectory = modPath.EndsWith(".dll") ? Path.GetDirectoryName(modPath) : modPath;
        return Path.Combine(modDirectory, "Content", content).Replace("\\", "/");
    }

    [Obsolete("Use Content.MapResources to look for resources")]
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
        if (atlases.TryGetValue(atlasID, out var atlasExisted)) 
            return atlasExisted;
        
        using var xml = this[contentPath + "/" + xmlPath].Stream;
        using var image = this[contentPath + "/" + imagePath].Stream;
        var atlas = AtlasExt.CreateAtlas(this, xml, image);
        atlases.Add(atlasID, atlas);
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

    /// <summary>
    /// Load a music form a mod content path. 
    /// This will use the <c>ContentPath</c> property, it's <c>Content</c> by default.
    /// </summary>
    /// <param name="fileName">A path to the .wav or .ogg file</param>
    /// <param name="musicType">A music type of how it will behave when played</param>
    /// <returns>A MusicHolder that can holds your music to be able to play in MusicPlayer</returns>
    public MusicHolder LoadMusic(string fileName, CustomMusicType musicType) 
    {
        return new MusicHolder(this, fileName, ContentAccess.ModContent, musicType);
    }

    public SFX LoadSFX(string fileName, bool obeysMasterPitch = true) 
    {
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXExt.CreateSFX(this, stream, obeysMasterPitch);
    }

    public patch_SFXInstanced LoadSFXInstance(string fileName, int instances = 2, bool obeysMasterPitch = true) 
    {
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXInstancedExt.CreateSFXInstanced(this, stream, instances, obeysMasterPitch);
    }

    public patch_SFXLooped LoadSFXLooped(string fileName, bool obeysMasterPitch = true) 
    {
        using var stream = this[contentPath + "/" + fileName].Stream;
        return SFXLoopedExt.CreateSFXLooped(this, stream, obeysMasterPitch);
    }

    public patch_SFXVaried LoadSFXVaried(string fileName, int amount, bool obeysMasterPitch = true) 
    {
        var currentExtension = Path.GetExtension(".wav");
        fileName = fileName.Replace(currentExtension, "");
        var stream = new Stream[amount];
        for (int i = 0; i < amount; i++) 
        {
            stream[i] = this[contentPath + "/" + fileName + SFXVariedExt.GetSuffix(i + 1) + currentExtension].Stream;
        }
        return SFXVariedExt.CreateSFXVaried(this, stream, amount, obeysMasterPitch);
    }

    public void Dispose()
    {
        ResourceSystem.Dispose();
        foreach (var atlas in atlases) 
        {
            atlas.Value.Texture2D.Dispose();
        }
        atlases.Clear();
    }
}

public class ModResource 
{
    public ModuleMetadata Metadata;
    public FortContent Content;
    public bool IsZip;

    public ModResource(FortContent content, ModuleMetadata metadata, bool zip = false) 
    {
        Metadata = metadata;
        Content = content;
        IsZip = zip;
    }
}

public class MusicHolder
{
    private string filePath;
    public string FilePath 
    {
        get 
        {
            return Access switch 
            {
                ContentAccess.Content => Calc.LOADPATH + FilePath,
                ContentAccess.ModContent => content + "/" + filePath,
                _ => filePath
            };
        }
        set => filePath = value;
    }
    public Stream Stream 
    {
        get 
        {
            if (content.MapResource.TryGetValue(FilePath, out var resource)) 
            {
                return resource.Stream;
            }
            return File.OpenRead(FilePath);
        }
    }
    public ContentAccess Access;
    public CustomMusicType MusicType;
    private FortContent content;


    public MusicHolder(FortContent content, string filePath, ContentAccess access, CustomMusicType musicType) 
    {
        this.content = content;
        FilePath = access switch 
        {
            ContentAccess.Content => Calc.LOADPATH + FilePath,
            ContentAccess.ModContent => content.UseContent + filePath,
            _ => filePath
        };
        Access = access;
        MusicType = musicType;
    }
}