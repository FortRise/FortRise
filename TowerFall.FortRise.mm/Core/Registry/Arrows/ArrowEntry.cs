#nullable enable
using TowerFall;

namespace FortRise;

internal class ArrowEntry : IArrowEntry
{
    public string Name { get; init; }
    public ArrowConfiguration Configuration { get; init; }
    public ArrowTypes ArrowTypes { get; init; }

    public ArrowEntry(string name, ArrowTypes arrowtypes, ArrowConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
        ArrowTypes = arrowtypes;
    }
}
