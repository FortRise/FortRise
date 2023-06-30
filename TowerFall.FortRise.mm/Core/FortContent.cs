using System;
using System.IO;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using Ionic.Zip;

namespace FortRise;

public class FortContent 
{
    public RiseCore.ResourceSystem ResourceSystem;
    private string modPath;

    public Dictionary<string, RiseCore.Resource> MapResource = new();

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
            MapResource = ResourceSystem.GetFilesystem(dir);
            return;
        }
        ResourceSystem = new RiseCore.FolderResourceSystem(dir);
        MapResource = ResourceSystem.GetFilesystem(dir);
    }

    internal void Unload() 
    {
        ResourceSystem.Dispose();
    }

    public string GetContentPath(string content) 
    {
        var modDirectory = modPath.EndsWith(".dll") ? Path.GetDirectoryName(modPath) : modPath;
        return Path.Combine(modDirectory, "Content", content).Replace("\\", "/");
    }


    public string GetContentPath() 
    {
        var modDirectory = modPath.EndsWith(".dll") ? Path.GetDirectoryName(modPath) : modPath;
        return Path.Combine(modDirectory, "Content").Replace("\\", "/");
    }

    public RiseCore.Resource[] GetFiles(string path) 
    {
        var folder = MapResource[path];
        return folder.Childrens.ToArray();
    }

    [Obsolete("Use LoadAtlas(string xmlPath, string imagePath) instead")]
    public patch_Atlas LoadAtlas(string xmlPath, string imagePath, bool load = true) 
    {
        return AtlasExt.CreateAtlas(this, xmlPath, imagePath, ContentAccess.ModContent);
    }

    public patch_Atlas LoadAtlas(string xmlPath, string imagePath) 
    {
        using var xml = MapResource["Content/" + xmlPath].Stream;
        using var image = MapResource["Content/" + imagePath].Stream;
        return AtlasExt.CreateAtlas(this, xml, image);
    }

    public patch_SpriteData LoadSpriteData(string filename, patch_Atlas atlas) 
    {
        using var xml = MapResource["Content/" + filename].Stream;
        return SpriteDataExt.CreateSpriteData(this, xml, atlas);
    }

    public Stream LoadStream(string path) 
    {
        var filePath = GetContentPath(path);
        return File.OpenRead(filePath);
    }

    public XNAGraphics::Texture2D LoadRawTexture2D(string path) 
    {
        using var stream = LoadStream(path);
        var tex2D = XNAGraphics::Texture2D.FromStream(Engine.Instance.GraphicsDevice, stream);
        return tex2D;
    }

    public Texture LoadTexture(string path) 
    {
        var tex2D = LoadRawTexture2D(path);
        var tex = new Texture(tex2D);
        return tex;
    }

    public MusicHolder LoadMusic(string fileName, CustomMusicType musicType) 
    {
        return new MusicHolder(this, fileName, ContentAccess.ModContent, musicType);
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
                ContentAccess.ModContent => content.GetContentPath(filePath),
                _ => filePath
            };
        }
        set => filePath = value;
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
            ContentAccess.ModContent => content.GetContentPath(filePath),
            _ => filePath
        };
        Access = access;
        MusicType = musicType;
    }
}