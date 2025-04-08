#nullable enable
namespace FortRise;

public interface ICommand 
{
    string Name { get; init; }
    public CommandConfiguration Configuration { get; init; }
}
