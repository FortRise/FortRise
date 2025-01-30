using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    public static partial class Events
    {
        public static class Player 
        {
            public delegate void OnPlayerSpawnHandler(TowerFall.Player player, int playerIndex);

            public static event OnPlayerSpawnHandler OnPlayerSpawn;

            internal static void Invoke_OnSpawn(TowerFall.Player player, int playerIndex) 
            {
                OnPlayerSpawn?.Invoke(player, playerIndex);
            }

            public delegate void OnPlayerDiedHandler(TowerFall.Player player, int playerIndex, DeathCause deathCause, int killerIndex, bool brambled, bool laser);

            public static event OnPlayerDiedHandler OnPlayerDie;

            internal static void Invoke_OnPlayerDie(TowerFall.Player player, int playerIndex, DeathCause deathCause, int killerIndex, bool brambled, bool laser) 
            {
                OnPlayerDie?.Invoke(player, playerIndex, deathCause, killerIndex, brambled, laser);
            }
        }
    }
}