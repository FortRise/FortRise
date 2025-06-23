#nullable enable
namespace FortRise;

internal class ModContent : IModContent
{
    public ModuleMetadata Metadata { get; init; }
    public IResourceInfo Root
    {
        get
        {
            return RiseCore.ResourceTree.Get($"mod:{Metadata.Name}/Content");
        }
    }


    public ModContent(ModuleMetadata metadata)
    {
        Metadata = metadata;
    }
}
