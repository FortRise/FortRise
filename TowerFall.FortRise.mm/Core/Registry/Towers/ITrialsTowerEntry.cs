#nullable enable
namespace FortRise;

public interface ITrialsTowerEntry : ITowerEntry
{
    public TrialsTowerConfiguration Configuration { get; init; }
}
