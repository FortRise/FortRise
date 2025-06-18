#nullable enable
using Monocle;

namespace FortRise;

public interface IBaseSFXEntry
{
    public string Name { get; init; }
    public bool ObeysMasterPitch { get; init; }
    public SFX? BaseSFX { get; }
}
