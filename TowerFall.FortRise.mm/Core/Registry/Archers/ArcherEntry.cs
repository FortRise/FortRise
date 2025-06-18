#nullable enable
namespace FortRise;

internal sealed class ArcherEntry : IArcherEntry
{
    public string Name { get; init; }
    public ArcherConfiguration Configuration { get; init; }
    public int Index { get; init; }
    public ArcherEntryType Type { get; init; }

    public ArcherEntry(string name, ArcherConfiguration configuration, int index, ArcherEntryType entryType)
    {
        Name = name;
        Configuration = configuration;
        Index = index;
        Type = entryType;
    }
}
