using Microsoft.Xna.Framework;

namespace TowerFall;

public class patch_Enemy : Enemy
{
    public patch_Enemy(Vector2 position, Facing facing, int states, int health, int bounty, params ArrowTypes[] arrows) : base(position, facing, states, health, bounty, arrows)
    {
    }

    /// <summary>
    /// Additional initialization method after the constructor was called.
    /// </summary>
    public virtual void Load() {}
}
