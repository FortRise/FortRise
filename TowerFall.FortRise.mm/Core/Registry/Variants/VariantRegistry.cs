using System.Collections.Generic;

namespace FortRise;

internal static class VariantRegistry 
{
    public static Dictionary<string, IVariantEntry> Variants = new Dictionary<string, IVariantEntry>();

    public static void Register(IVariantEntry variant)
    {
        Variants.Add(variant.Name, variant);
    }
}
