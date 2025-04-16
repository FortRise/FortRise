#nullable enable
using TowerFall;

namespace FortRise;

internal class ArrowEntry : IArrowEntry
{
    public string Name { get; init; }
    public ArrowConfiguration Configuration { get; init; }


    public ArrowEntry(string name, ArrowConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public ArrowTypes ToArrowTypes()
    {
        return ModRegisters.ArrowType(Name);
    }
}
