#nullable enable
using TowerFall;

namespace FortRise;

public interface IArcherEntry
{
    public string Name { get; init; }
    public ArcherConfiguration Configuration { get; init; }
    public int Index { get; init; }
    public ArcherEntryType Type { get; init; }
    public ArcherData? ArcherData { get; }
}


public enum ArcherEntryType
{
    Normal,
    Alt,
    Secret
}