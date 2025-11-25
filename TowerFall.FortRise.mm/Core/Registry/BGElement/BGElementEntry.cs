namespace FortRise;

internal sealed class BGElementEntry(string name, BGElementConfiguration configuration) : IBGElementEntry
{
    public string Name { get; init; } = name;
    public BGElementConfiguration Configuration { get; init; } = configuration;
}
