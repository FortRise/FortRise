using System.Collections.Generic;

namespace FortRise;

internal static class PresetRegistry 
{
    public static List<IVariantPresetEntry> Presets = new List<IVariantPresetEntry>();

    public static void Register(IVariantPresetEntry presetEntry)
    {
        Presets.Add(presetEntry);
    }
}