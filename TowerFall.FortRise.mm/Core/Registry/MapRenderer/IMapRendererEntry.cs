namespace FortRise;

public interface IMapRendererEntry
{
    string Name { get; init; }
    MapRendererConfiguration Configuration { get; init; }
}

