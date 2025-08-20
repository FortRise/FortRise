namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    IArcherAPI Archers { get; }
    ITilesetsAPI Tilesets { get; }
}
