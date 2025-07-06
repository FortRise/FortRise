#nullable enable
using TowerFall;

namespace FortRise;

public interface IVersusTowerEntry : ITowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
    public VersusTowerData? VersusTowerData { get; }
}
