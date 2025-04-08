using System.Collections.Generic;

namespace FortRise;

public static class VariantRegistry 
{
    public static Dictionary<string, IVariant> Variants = new Dictionary<string, IVariant>();
    public static List<IVariant> CanRandoms = new List<IVariant>();

    public static void Register(IVariant variant)
    {
        Variants.Add(variant.Name, variant);
    }
}