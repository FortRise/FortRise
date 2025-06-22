#nullable enable
using System.Xml;
using Monocle;

namespace FortRise;

public interface ISpriteEntry
{
    /// <summary>
    /// A sprite identity for SpriteData.
    /// </summary>
    public string ID { get; init; }
    public XmlElement Xml { get; }
}

public interface ISpriteEntry<T> : ISpriteEntry
{
    public Sprite<T>? Sprite { get; }
    public SpriteConfiguration<T> Configuration { get; init; }
}