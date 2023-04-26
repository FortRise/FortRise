#pragma warning disable CS0626
#pragma warning disable CS0108
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldRoundLogic : RoundLogic
{
    public patch_DarkWorldRoundLogic(Session session) : base(session, false)
    {
    }

    [MonoModIgnore]
    [PatchDarkWorldRoundLogicOnPlayerDeath]
    public override extern void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex);
}