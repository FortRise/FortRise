#nullable enable
namespace FortRise;

public interface ICommandEntry 
{
    string Name { get; init; }
    public CommandConfiguration Configuration { get; init; }
}
