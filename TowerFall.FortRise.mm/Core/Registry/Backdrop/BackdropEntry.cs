namespace FortRise;

internal sealed class BackdropEntry(string name, BackdropConfiguration configuration) : IBackdropEntry
{
    public string Name { get; init; } = name;
    public BackdropConfiguration Configuration { get; init; } = configuration;
}
