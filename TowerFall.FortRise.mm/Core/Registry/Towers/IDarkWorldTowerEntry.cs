#nullable enable
namespace FortRise;

public interface IDarkWorldTowerEntry : ITowerEntry
{
    public DarkWorldTowerConfiguration Configuration { get; init; }    
}
