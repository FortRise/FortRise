using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall 
{
    public class patch_RoundLogic : RoundLogic
    {
        public patch_RoundLogic(Session session, bool canHaveMiasma) : base(session, canHaveMiasma)
        {
        }

        public extern static RoundLogic orig_GetRoundLogic(patch_Session session);

        public static RoundLogic GetRoundLogic(patch_Session session)
        {
            if (session.MatchSettings.IsCustom)
            {
                var gamemode = session.MatchSettings.CustomVersusGameMode;
                if (gamemode != null)
                {
                    return gamemode.OnCreateRoundLogic(session);
                }
            }
            return orig_GetRoundLogic(session);
        }

        [MonoModReplace]
        public override void OnLevelLoadFinish()
        {
            if (!Session.MatchSettings.SoloMode)
            {
                SaveData.Instance.Stats.RoundsPlayed++;
                SessionStats.RoundsPlayed++;
            }

            ModEventsManager.Instance.RoundLogicLevelLoadFinish.Raise(this, this);
        }

        [MonoModIgnore]
        [MonoModIfFlag("NoLauncher")]
        [FixGameStatsTotalVersusKills]
		public extern override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex);
    }
}
