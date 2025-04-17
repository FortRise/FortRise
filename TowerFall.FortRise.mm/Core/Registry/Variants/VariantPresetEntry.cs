#nullable enable
namespace FortRise;

internal sealed class VariantPresetEntry : IVariantPresetEntry
{
    public string Name { get; init; }

    public PresetConfiguration Configuration { get; init; }

    public VariantPresetEntry(string name, PresetConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }
}