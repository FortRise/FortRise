#nullable enable
namespace FortRise;

internal class TilesetEntry : ITilesetEntry
{
    public string Name { get; init; }
    public TilesetConfiguration Configuration { get; init; }

    public TilesetEntry(string name, TilesetConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}
