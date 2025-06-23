#nullable enable
using Monocle;

namespace FortRise;

/// <summary>
/// A container for the Subtexture.
/// </summary>
public interface ISubtextureEntry
{
    /// <summary>
    /// A subtexture identity for Atlas.
    /// </summary>
    public string ID { get; init; }
    /// <summary>
    /// A direct resource path to the texture.
    /// </summary>
    public IResourceInfo? Path { get; init; }
    /// <summary>
    /// An actual subtexture to be used.
    /// </summary>
    public Subtexture? Subtexture { get; }
    /// <summary>
    /// The destination to put this subtexture on.
    /// </summary>
    public SubtextureAtlasDestination AtlasDestination { get; }
}
