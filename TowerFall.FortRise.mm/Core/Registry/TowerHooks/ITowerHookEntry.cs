#nullable enable
namespace FortRise;

public interface ITowerHookEntry 
{
    string Name { get; }
    ITowerHook Hook { get; }
}
