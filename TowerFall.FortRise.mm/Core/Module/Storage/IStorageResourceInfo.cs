#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

namespace FortRise;

public interface IStorageResourceInfo
{
    string FullPath { get; internal set; }
    string Path { get; internal set; }
    string Name => System.IO.Path.GetFileName(Path);

    string Text { get; }
    XmlDocument? Xml { get; }


    IReadOnlyList<IStorageResourceInfo> Childrens { get; }
    Stream ReadStream { get; }
    Stream WriteStream { get; }

    IStorageResourceInfo GetRelativePath(string path);
    bool TryGetRelativePath(string path, [NotNullWhen(true)] out IStorageResourceInfo resource);
    bool ExistsRelativePath(string path);

    IStorageResourceInfo AddFile(string filename);
}
