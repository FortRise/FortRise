#nullable enable
namespace FortRise;

public interface IModContent
{
    public ModuleMetadata Metadata { get; init; }
    /// <summary>
    /// The "Content" folder from a mod resource.
    /// </summary>
    public IResourceInfo Root { get; }
}
