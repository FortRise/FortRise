#nullable enable
namespace FortRise;

internal class ThemeEntry : IThemeEntry
{
    public string Name { get; init; }
    public ThemeConfiguration Configuration { get; init; }

    public ThemeEntry(string name, ThemeConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}
