#nullable enable
using TowerFall;

namespace FortRise;

public interface IArrowEntry
{
    string Name { get; init; }
    public ArrowConfiguration Configuration { get; init; }

    public ArrowTypes ArrowTypes { get; init; }
}
