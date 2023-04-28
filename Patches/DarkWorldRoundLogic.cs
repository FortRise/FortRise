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

    [PatchDarkWorldRoundLogicOnPlayerDeath]
    public extern void orig_OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex);

    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        if (patch_SaveData.AdventureActive)
            patch_GameData.AdventureWorldTowers[Session.MatchSettings.LevelSystem.ID.X].Stats.Deaths += 1;
        orig_OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
    }
}