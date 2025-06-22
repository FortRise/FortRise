#nullable enable
namespace FortRise;

public interface IBaseSpriteContainerEntry
{
    public ISpriteEntry Entry { get; init; }
    public ContainerSpriteType Type { get; init; }
}

public interface ISpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IMenuSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface ICorpseSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IBGSpriteContainerEntry : IBaseSpriteContainerEntry;
public interface IBossSpriteContainerEntry : IBaseSpriteContainerEntry;