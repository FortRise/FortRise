#nullable enable
using TowerFall;

namespace FortRise;

internal class ArrowMetadata : IArrow
{
    public string Name { get; init; }
    public ArrowConfiguration Configuration { get; init; }


    public ArrowMetadata(string name, ArrowConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public ArrowTypes ToArrowTypes()
    {
        return ModRegisters.ArrowType(Name);
    }
}
