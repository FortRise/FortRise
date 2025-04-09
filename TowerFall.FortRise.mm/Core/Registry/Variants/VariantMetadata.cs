#nullable enable
using TowerFall;

namespace FortRise;

internal class VariantMetadata : IVariant
{
    public string Name { get; init; }
    public VariantConfiguration Configuration { get; init; }


    public VariantMetadata(string name, VariantConfiguration configuration)
    {
        Name = name;
        Configuration = configuration;
    }

    public bool IsActive(MatchVariants matchVariants)
    {
        if (((patch_MatchVariants)matchVariants).CustomVariants.TryGetValue(Name, out Variant? variant))
        {
            return variant;
        }
        return false;
    }

    public bool IsActive(MatchVariants matchVariants, int playerIndex)
    {
        if (((patch_MatchVariants)matchVariants).CustomVariants.TryGetValue(Name, out Variant? variant))
        {
            return variant[playerIndex];
        }
        return false;
    }

    public bool IsActive()
    {
        var matchVariants = (MainMenu.CurrentMatchSettings.Variants as patch_MatchVariants)!;
        if (matchVariants.CustomVariants.TryGetValue(Name, out Variant? variant))
        {
            return variant;
        }
        return false;
    }

    public bool IsActive(int playerIndex)
    {
        var matchVariants = (MainMenu.CurrentMatchSettings.Variants as patch_MatchVariants)!;
        if (matchVariants.CustomVariants.TryGetValue(Name, out Variant? variant))
        {
            return variant[playerIndex];
        }
        return false;
    }
}
