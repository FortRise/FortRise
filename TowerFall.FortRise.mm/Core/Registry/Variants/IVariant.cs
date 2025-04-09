#nullable enable
using TowerFall;

namespace FortRise;

public interface IVariant 
{
    string Name { get; init; }
    public VariantConfiguration Configuration { get; init; }
    bool IsActive(MatchVariants matchVariants);
    bool IsActive(MatchVariants matchVariants, int playerIndex);
    bool IsActive(int playerIndex);
    bool IsActive();
}
