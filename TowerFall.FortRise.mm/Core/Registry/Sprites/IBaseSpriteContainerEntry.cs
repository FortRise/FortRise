#nullable enable
namespace FortRise;

/// <summary>
/// A base for all concrete container entry types.
/// </summary>
public interface IBaseSpriteContainerEntry
{
    /// <summary>
    /// Get an entry without a concrete type. Must cast with a concrete type to use its other properties.
    /// </summary>
    public ISpriteEntry Entry { get; init; }
    /// <summary>
    /// A <see cref="Monocle.SpriteData"/> container type where this sprite is injected to.
    /// </summary>
    public ContainerSpriteType Type { get; init; }

    /// <summary>
    /// Get an entry with a concrete type.
    /// </summary>
    /// <typeparam name="T">A supported type, either an int or string</typeparam>
    /// <returns>A concrete <see cref="ISpriteEntry{T}"/></returns>
    ISpriteEntry<T> GetCastEntry<T>()
    {
        return (SpriteEntry<T>)Entry;
    }
}

/// <summary>
/// A sprite container that is located from <see cref="TowerFall.TFGame.SpriteData"/>.
/// </summary>
public interface ISpriteContainerEntry : IBaseSpriteContainerEntry;

/// <summary>
/// A sprite container that is located from <see cref="TowerFall.TFGame.MenuSpriteData"/>.
/// </summary>
public interface IMenuSpriteContainerEntry : IBaseSpriteContainerEntry;

/// <summary>
/// A sprite container that is located from <see cref="TowerFall.TFGame.CorpseSpriteData"/>.
/// </summary>
public interface ICorpseSpriteContainerEntry : IBaseSpriteContainerEntry;

/// <summary>
/// A sprite container that is located from <see cref="TowerFall.TFGame.BGSpriteData"/>.
/// </summary>
public interface IBGSpriteContainerEntry : IBaseSpriteContainerEntry;

/// <summary>
/// A sprite container that is located from <see cref="TowerFall.TFGame.BossSpriteData"/>.
/// </summary>
public interface IBossSpriteContainerEntry : IBaseSpriteContainerEntry;