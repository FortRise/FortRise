#pragma warning disable CS0626
#pragma warning disable CS0108
using MonoMod;

namespace TowerFall;

public class patch_Session : Session
{
    public patch_Session(MatchSettings settings) : base(settings)
    {
    }

    [PatchSessionStartGame]
    public extern void orig_StartGame();

    public void StartGame() 
    {
        orig_StartGame();
        var worldTower = patch_GameData.AdventureWorldTowers[MatchSettings.LevelSystem.ID.X];
        worldTower.Stats.Attempts += 1;
    }
}