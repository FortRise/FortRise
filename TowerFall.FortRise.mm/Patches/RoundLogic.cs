using System;
using FortRise;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

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
        [MonoModPatch("OnLevelLoadFinish")]
        [MonoModIfFlag("Steamworks")]
        public virtual void OnLevelLoadFinish_Steamworks()
        {
            if (!Session.MatchSettings.SoloMode)
            {
                SaveData.Instance.Stats.RoundsPlayed++;
                SessionStats.RoundsPlayed++;
            }

            ModEventsManager.Instance.OnLevelLoaded.Raise(this, this);
        }

        [MonoModReplace]
        [MonoModPatch("OnLevelLoadFinish")]
        [MonoModIfFlag("NoLauncher")]
        public virtual void OnLevelLoadFinish_NoLauncher()
        {
            if (!Session.MatchSettings.SoloMode)
            {
                SessionStats.RoundsPlayed++;
            }

            ModEventsManager.Instance.OnLevelLoaded.Raise(this, this);
        }
    }
}