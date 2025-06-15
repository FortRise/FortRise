#nullable enable
using System.Xml;
using Microsoft.Xna.Framework;

namespace FortRise;

public interface ITilesetEntry
{
    public string Name { get; init; }
    public TilesetConfiguration Configuration { get; init; }
}
