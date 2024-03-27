using FortRise.Adventure;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldRoundLogic : DarkWorldRoundLogic
{
    private float autoReviveCounter;

    public patch_DarkWorldRoundLogic(Session session) : base(session)
    {
    }

    [MonoModLinkTo("TowerFall.RoundLogic", "OnPlayerDeath")]
    [MonoModIgnore]
    public void base_OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex) 
    {
        base.OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
    }

    [MonoModReplace]
    public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    {
        base_OnPlayerDeath(player, corpse, playerIndex, cause, position, killerIndex);
        
        if ((Session.MatchSettings.LevelSystem as DarkWorldLevelSystem).DarkWorldTowerData.GetLevelSet() != "TowerFall")
        {
            var tower = TowerRegistry.DarkWorldGet(Session.GetLevelSet(), Session.MatchSettings.LevelSystem.ID.X);
            tower.Stats.Deaths += 1;
        }
        else 
        {
            SaveData.Instance.DarkWorld.Towers[base.Session.MatchSettings.LevelSystem.ID.X].Deaths += 1UL;
        }
        Session.DarkWorldState.OnPlayerDeath(player);
        if (!Control.PlayerEnteredPortal && !Session.CurrentLevel.Ending && CoOpCheckForAllDead())
        {
            if (Session.DarkWorldState.ExtraLives > 0)
            {
                autoReviveCounter = 60f;
                return;
            }
            if (Session.DarkWorldState.ContinuesRemaining == 0)
            {
                FinalKillNoSpotlight();
            }
            else
            {
                FinalKillNoSpotlightOrMusicStop();
            }
            Session.CurrentLevel.Ending = true;
            Session.CurrentLevel.Add<DarkWorldGameOver>(new DarkWorldGameOver(this));
        }
    }
}