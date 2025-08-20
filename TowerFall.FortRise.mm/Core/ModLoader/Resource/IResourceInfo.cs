#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    internal void AssignType();
    IResourceInfo GetRelativePath(string path);
    bool TryGetRelativePath(string path, [NotNullWhen(true)] out IResourceInfo resource);

    bool ExistsRelativePath(string path);
    IEnumerable<IResourceInfo> EnumerateChildrens(string pattern);
}
