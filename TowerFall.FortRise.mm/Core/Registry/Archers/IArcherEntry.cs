#nullable enable
namespace FortRise;

public interface IArcherEntry
{
    public string Name { get; init; }
    public ArcherConfiguration Configuration { get; init; }
    public int Index { get; init; }
    public ArcherEntryType Type { get; init; }
}


public enum ArcherEntryType
{
    Normal,
    Alt,
    Secret
}