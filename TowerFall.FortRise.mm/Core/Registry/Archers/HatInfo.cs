#nullable enable
using TowerFall;

namespace FortRise;

public readonly struct HatInfo
{
    public ArcherData.HatMaterials Material { get; init; }
    public ISubtextureEntry? Normal { get; init; }
    public ISubtextureEntry? Blue { get; init; }
    public ISubtextureEntry? Red { get; init; }
}
