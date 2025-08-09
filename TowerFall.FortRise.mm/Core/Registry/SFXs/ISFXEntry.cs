#nullable enable
using Monocle;

namespace FortRise;

public interface ISFXEntry : IBaseSFXEntry
{
    public IResourceInfo? Path { get; init; }
    public SFX? SFX { get; }
}
