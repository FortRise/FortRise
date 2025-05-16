using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FortRise;

/// <summary>
/// A class that contains a path and stream to your resource works both on folder and zip.
/// </summary>
public abstract class ResourceInfo : IResourceInfo
{
    public string FullPath;
    public string Path;
    public string Root;
    public string RootPath => Root + Path;
    public List<IResourceInfo> Childrens = new();
    public IModResource Source;
    public Type ResourceType;

    public abstract Stream Stream { get; }

    string IResourceInfo.FullPath
    {
        get => FullPath;
        set => FullPath = value;
    }

    string IResourceInfo.Path
    {
        get => Path;
        set => Path = value;
    }

    string IResourceInfo.Root
    {
        get => Root;
        set => Root = value;
    }

    IReadOnlyList<IResourceInfo> IResourceInfo.Childrens => Childrens;

    IModResource IResourceInfo.Resource => Source;

    Type IResourceInfo.ResourceType => ResourceType;

    public string Text => ModIO.ReadAllText(this);

#nullable enable
    public XmlDocument? Xml
    {
        get
        {
            try
            {
                var xml = ModIO.LoadXml(this);
                return xml;
            }
            catch (XmlException ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }
    }
#nullable disable

    public ResourceInfo(IModResource resource, string path, string fullPath)
    {
        FullPath = fullPath;
        Path = path;
        Source = resource;
    }

    public IResourceInfo GetRelativePath(string path)
    {
        string actualPath = System.IO.Path.Combine(RootPath, path);
        return RiseCore.ResourceTree.Get(actualPath);
    }

    public bool TryGetRelativePath(string path, out IResourceInfo resource)
    {
        string actualPath = System.IO.Path.Combine(RootPath, path);
        return RiseCore.ResourceTree.TryGetValue(path, out resource);
    }

    public bool ExistsRelativePath(string path)
    {
        string actualPath = System.IO.Path.Combine(RootPath, path);
        return RiseCore.ResourceTree.IsExist(actualPath);
    }

    public void AssignType()
    {
        var path = Path;
        var filename = System.IO.Path.GetFileName(path);


        if (path.StartsWith("Content/Atlas/atlas/") && filename.EndsWith(".png"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeAtlasPng);
        }
        else if (path.StartsWith("Content/Atlas/menuAtlas/") && filename.EndsWith(".png"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeMenuAtlasPng);
        }
        else if (path.StartsWith("Content/Atlas/bossAtlas/") && filename.EndsWith(".png"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeBossAtlasPng);
        }
        else if (path.StartsWith("Content/Atlas/bgAtlas/") && filename.EndsWith(".png"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeBGAtlasPng);
        }
        else if (path.StartsWith("Content/Atlas") && filename.EndsWith(".png"))
        {
            foreach (var ext in AtlasReader.InternalReaders.Keys)
            {
                if (RiseCore.ResourceTree.IsExist(this, path.Replace(".png", ext)))
                {
                    ResourceType = typeof(RiseCore.ResourceTypeAtlas);
                }
            }

        }
        else if (path.EndsWith(".dll"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeAssembly);
        }
        else if (path.StartsWith("Content/Atlas/GameData") && filename.EndsWith(".xml"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeGameData);
        }
        else if (path.StartsWith("Content/Atlas/SpriteData") && filename.EndsWith(".xml"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeSpriteData);
        }
        else if (path.StartsWith("Content/Levels/DarkWorld"))
        {
            if (RiseCore.ResourceTree.IsExist(this, path + "/tower.xml"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeDarkWorldTowerFolder);
            }
            else AssignLevelFile();
        }
        else if (path.StartsWith("Content/Levels/Versus"))
        {
            if (RiseCore.ResourceTree.IsExist(this, path + "/tower.xml"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeVersusTowerFolder);
            }
            else AssignLevelFile();
        }
        else if (path.StartsWith("Content/Levels/Quest"))
        {
            if (RiseCore.ResourceTree.IsExist(this, path + "/tower.xml"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeQuestTowerFolder);
            }
            else AssignLevelFile();
        }
        else if (path.StartsWith("Content/Levels/Trials"))
        {
            if (RiseCore.ResourceTree.IsExist(this, path + "/tower.xml"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeTrialsTowerFolder);
            }
            else AssignLevelFile();
        }
        else if (path.StartsWith("Content/Music"))
        {
            if (path.EndsWith(".xgs"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeAudioEngine);
            }
            else if (path.EndsWith(".xsb"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeSoundBank);
            }
            else if (path.EndsWith("xwb"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeWaveBank);
            }
            else if (path.EndsWith(".wav"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeWavFile);
            }
            else if (path.EndsWith(".ogg"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeOggFile);
            }
            // FIXME fix normal file
            else
            {
                ResourceType = typeof(RiseCore.ResourceTypeFile);
            }
        }
        else if (path.StartsWith("Content/SFX"))
        {
            if (path.EndsWith(".wav"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeWavFile);
            }
            else if (path.EndsWith(".ogg"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeOggFile);
            }
            else if (path.EndsWith("SoundBank.xml"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeXMLSoundBank);
            }
            else if (path.EndsWith("SoundBank.json"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeJSONSoundBank);
            }
            // FIXME fix normal file
            else
            {
                ResourceType = typeof(RiseCore.ResourceTypeFile);
            }
        }
        else if (path.EndsWith(".xml"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeXml);
        }
        else if (path.EndsWith(".fxb"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeEffects);
        }
        else if (Childrens.Count != 0)
        {
            ResourceType = typeof(RiseCore.ResourceTypeFolder);
        }
        else
        {
            ResourceType = typeof(RiseCore.ResourceTypeFile);
        }
        RiseCore.Events.Invoke_OnResourceAssignType(path, filename, ref ResourceType);

        void AssignLevelFile()
        {
            if (path.EndsWith(".json"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeJson);
            }

            else if (path.EndsWith(".oel"))
            {
                ResourceType = typeof(RiseCore.ResourceTypeOel);
            }
        }
    }
}
