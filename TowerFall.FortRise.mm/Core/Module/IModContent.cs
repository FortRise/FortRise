#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FortRise;

public interface IModContent
{
    public ModuleMetadata Metadata { get; init; }
    /// <summary>
    /// The "Content" folder from a mod resource.
    /// </summary>
    public IResourceInfo Root { get; }

    Stream OpenStream(string resourcePath);

    IResourceInfo GetResource(string resourcePath);
    bool TryGetResource(string resourcePath, [NotNullWhen(true)] out IResourceInfo info);
}
