using System;
using System.IO;
using XNAGraphics = Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise;

public class FortContent 
{
    private string modPath;
    public FortContent(FortModule module) 
    {
        modPath = module.Meta.DLL;
    }

    public FortContent(string pathDirectory) 
    {
        modPath = pathDirectory;
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

    [Obsolete("Use LoadAtlas(string xmlPath, string imagePath) instead")]
    public patch_Atlas LoadAtlas(string xmlPath, string imagePath, bool load = true) 
    {
        return AtlasExt.CreateAtlas(this, xmlPath, imagePath, ContentAccess.ModContent);
    }

    public patch_Atlas LoadAtlas(string xmlPath, string imagePath) 
    {
        return AtlasExt.CreateAtlas(this, xmlPath, imagePath, ContentAccess.ModContent);
    }

    public patch_SpriteData LoadSpriteData(string filename, patch_Atlas atlas) 
    {
        return SpriteDataExt.CreateSpriteData(this, filename, atlas, ContentAccess.ModContent);
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

    public ModResource(FortContent content, ModuleMetadata metadata) 
    {
        Metadata = metadata;
        Content = content;
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