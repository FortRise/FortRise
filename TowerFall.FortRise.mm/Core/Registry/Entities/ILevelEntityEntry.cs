#nullable enable
namespace FortRise;

public interface ILevelEntityEntry
{
    public string ID { get; init; }
    public LevelEntityConfiguration Configuration { get; init; }
}
