namespace FortRise;

internal sealed class MapRendererEntry : IMapRendererEntry
{
    public string Name { get; init; }
    public MapRendererConfiguration Configuration { get; init; }


    public MapRendererEntry(string name, MapRendererConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}

