#nullable enable
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public interface IVersusGameMode 
{
    string Name { get; }
    Color NameColor => Color.White;
    Subtexture Icon { get; }
    bool IsTeamMode { get; }

    void OnStartGame(Session session);
    RoundLogic OnCreateRoundLogic(Session session);
    Sprite<int> OverrideCoinSprite(Session session) => VersusCoinButton.GetCoinSprite();
    int OverrideCoinOffset(Session? session) => 10;
    SFX OverrideEarnedCoinSFX(Session session) => Sounds.sfx_multiCoinEarned;
    SFX OverrideLoseCoinSFX(Session session) => Sounds.sfx_multiSkullNegative;
    bool IsRespectFixedFirst(MatchSettings matchSettings) => false;
    int GetMinimumTeamPlayers(MatchSettings matchSettings) => 3;
    int GetMinimumPlayers(MatchSettings matchSettings) => 2;
}
