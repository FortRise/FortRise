#nullable enable
using Monocle;

namespace FortRise;

public interface ISFXLoopedEntry : IBaseSFXEntry
{
    public IResourceInfo? Path { get; init; }
    public patch_SFXLooped? SFXLooped { get; }
}
