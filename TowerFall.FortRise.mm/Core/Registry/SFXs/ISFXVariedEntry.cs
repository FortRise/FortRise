#nullable enable
using Monocle;

namespace FortRise;

public interface ISFXVariedEntry : IBaseSFXEntry
{
    public IResourceInfo[] Variations { get; init; }
    public patch_SFXVaried? SFXVaried { get; }
    public int Count { get; init; }
}
