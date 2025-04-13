#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace FortRise;

public interface IResourceInfo 
{
    string FullPath { get; internal set; }
    string Path { get; internal set; }
    string Root { get; internal set; }
    string RootPath => Root + Path;
    string Name => System.IO.Path.GetFileName(Path);

    string Text { get; }
    XmlDocument? Xml { get; }


    IReadOnlyList<IResourceInfo> Childrens { get; }
    IModResource Resource { get; }
    Type ResourceType { get; }
    Stream Stream { get; }

    void AssignType();
    IResourceInfo GetRelativePath(string path);
}

public interface IModResource : IDisposable
{
    ModuleMetadata Metadata { get; }
    FortContent Content { get; }
    Dictionary<string, IResourceInfo> OwnedResources { get; }

    internal void Lookup(string prefix);
}

public interface IResourceLoader 
{
    void LoadResource(IModResource modResource);
}
