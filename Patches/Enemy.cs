using FortRise;
using Microsoft.Xna.Framework;

namespace TowerFall;

public class patch_Enemy : Enemy
{
    public patch_Enemy(Vector2 position, Facing facing, int states, int health, int bounty, params ArrowTypes[] arrows) : base(position, facing, states, health, bounty, arrows)
    {
    }

    public virtual void Load(EnemyDataArg args) {}
}

public static class TemporaryVariants
{

}