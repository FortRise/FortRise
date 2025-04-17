#nullable enable
namespace FortRise;

public interface IVariantPresetEntry 
{
    string Name { get; }
    PresetConfiguration Configuration { get; }
}
