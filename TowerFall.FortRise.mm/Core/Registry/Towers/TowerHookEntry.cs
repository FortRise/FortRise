#nullable enable
namespace FortRise;

internal class TowerHookEntry(string name, ITowerHook hook) : ITowerHookEntry
{
    public string Name { get; init; } = name;

    public ITowerHook Hook { get; init; } = hook;
}