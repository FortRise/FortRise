#nullable enable
namespace FortRise;

public interface IVersusTowerEntry : ITowerEntry
{
    public VersusTowerConfiguration Configuration { get; init; }
}
