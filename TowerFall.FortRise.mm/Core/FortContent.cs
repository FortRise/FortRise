using System.IO;
using Monocle;

namespace FortRise;

public class FortContent 
{
    private string dllPath;
    public FortContent(FortModule module) 
    {
        dllPath = module.Meta.DLL;
    }

    public string GetContentPath(string content) 
    {
        var modDirectory = Path.GetDirectoryName(dllPath);
        return Path.Combine(modDirectory, "Content", content).Replace("\\", "/");
    }

    public patch_Atlas LoadAtlas(string xmlPath, string imagePath, bool load = true) 
    {
        return AtlasExt.CreateAtlas(this, xmlPath, imagePath, load, ContentAccess.ModContent);
    }

    public patch_SpriteData LoadSpriteData(string filename, Atlas atlas) 
    {
        return SpriteDataExt.CreateSpriteData(this, filename, atlas, ContentAccess.ModContent);
    }

    public MusicHolder LoadMusic(string fileName, CustomMusicType musicType) 
    {
        return new MusicHolder(this, fileName, ContentAccess.ModContent, musicType);
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