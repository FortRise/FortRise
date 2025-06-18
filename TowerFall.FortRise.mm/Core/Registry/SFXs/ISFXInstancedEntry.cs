#nullable enable
using Monocle;

namespace FortRise;

public interface ISFXInstancedEntry : IBaseSFXEntry
{
    public IResourceInfo Path { get; init; }
    public int Instances { get; init; }
    public patch_SFXInstanced? SFXInstanced { get; }
}
