using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        return RiseCore.ResourceTree.TryGetValue(actualPath, out resource);
    }

    public bool ExistsRelativePath(string path)
    {
        string actualPath = System.IO.Path.Combine(RootPath, path);
        return RiseCore.ResourceTree.IsExist(actualPath);
    }

    public IEnumerable<IResourceInfo> EnumerateChildrens(string pattern)
    {
        foreach (var child in Source.OwnedResources.Values)
        {
            if (IsWildCardMatch(child.Path, pattern) == 1)
            {
                yield return child;
            }
        }
    }

    public void AssignType()
    {
        var path = Path;
        var filename = System.IO.Path.GetFileName(path);


        if (filename.EndsWith(".png"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeAtlasPng);
        }
        else if (path.EndsWith(".dll"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeAssembly);
        }

        else if (path.EndsWith(".ogg"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeOggFile);
        }

        else if (path.EndsWith(".wav"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeWavFile);
        }

        else if (path.EndsWith(".json"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeJson);
        }
        else if (path.EndsWith(".oel"))
        {
            ResourceType = typeof(RiseCore.ResourceTypeOel);
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
    }

    private static byte IsWildCardMatch(string text, string pattern)
    {
        int n = text.Length;
        int m = pattern.Length;

        int length = m + 1 + m + 1;

        Span<byte> buffer = length < 4096 ? stackalloc byte[length] : new byte[length];

        buffer[0] = 1;

        for (int j = 1; j <= m; j += 1)
        {
            if (pattern[j - 1] == '*')
            {
                buffer[j] = buffer[j - 1];
            }
        }

        for (int i = 1; i <= n; i += 1)
        {
            for (int j = 1; j <= m; j += 1)
            {
                char pat = pattern[j - 1];

                if (pat == text[i - 1] || pat == '?')
                {
                    buffer[m + 1 + j] = buffer[j - 1];
                    continue;
                }

                if (pat == '*')
                {
                    buffer[m + 1 + j] = (byte)(buffer[m + 1 + j - 1] | buffer[j]);
                    continue;
                }

                buffer[m + 1 + j] = 0;
            }

            unsafe 
            {
                fixed (byte *ptr = buffer)
                {
                    NativeMemory.Copy(&ptr[m + 1], ptr, (nuint)m + 1);
                }
            }
        }

        return buffer[m];
    }
}
