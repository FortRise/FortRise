namespace FortRise.Content;

internal sealed partial class ApiImplementation : IFortRiseContentApi
{
    public ApiImplementation() {}

    public IFortRiseContentApi.IArcherAPI Archers { get; } = new ArcherAPI();
    public IFortRiseContentApi.ITilesetsAPI Tilesets { get; } = new TilesetsAPI();
    public IFortRiseContentApi.ILoaderAPI LoaderApi { get; } = new LoaderAPI();
}
