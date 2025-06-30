#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FortRise;

internal class ModContent : IModContent
{
    public ModuleMetadata Metadata { get; init; }
    public IResourceInfo Root
    {
        get
        {
            return RiseCore.ResourceTree.Get($"mod:{Metadata.Name}/");
        }
    }


    public ModContent(ModuleMetadata metadata)
    {
        Metadata = metadata;
    }

    public Stream OpenStream(string resourcePath)
    {
        return Root.GetRelativePath(resourcePath).Stream;
    }

    public bool TryGetResource(string resourcePath, [NotNullWhen(true)] out IResourceInfo info)
    {
        return Root.TryGetRelativePath(resourcePath, out info);
    }

    public IResourceInfo GetResource(string resourcePath)
    {
        return Root.GetRelativePath(resourcePath);
    }
}
