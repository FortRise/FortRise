#nullable enable
namespace FortRise;

internal sealed class SpriteContainerEntry :
    ISpriteContainerEntry,
    IMenuSpriteContainerEntry,
    ICorpseSpriteContainerEntry,
    IBGSpriteContainerEntry,
    IBossSpriteContainerEntry
{
    public ISpriteEntry Entry { get; init; }
    public ContainerSpriteType Type { get; init; }

    public SpriteContainerEntry(ISpriteEntry entry, ContainerSpriteType type)
    {
        Entry = entry;
        Type = type;
    }
}
