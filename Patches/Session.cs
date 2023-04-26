#pragma warning disable CS0626
#pragma warning disable CS0108
using Monocle;

namespace TowerFall;

public class patch_Session : Session
{
    public TreasureSpawner TreasureSpawner { get; set; }
    public patch_Session(MatchSettings settings) : base(settings)
    {
    }

    public extern void orig_StartGame();

    //TODO ILFied
    public void StartGame() 
    {
        if (!this.MatchSettings.SoloMode)
        {
            GameStats stats = SaveData.Instance.Stats;
            int num = stats.MatchesPlayed;
            stats.MatchesPlayed = num + 1;
            SaveData.Instance.Stats.VersusTowerPlays[this.MatchSettings.LevelSystem.ID.X] += 1UL;
            if (this.MatchSettings.RandomVersusTower)
            {
                GameStats stats2 = SaveData.Instance.Stats;
                num = stats2.VersusRandomPlays;
                stats2.VersusRandomPlays = num + 1;
            }
            else
            {
                SaveData.Instance.Stats.RegisterVersusTowerSelection(this.MatchSettings.LevelSystem.ID.X);
            }
            SessionStats.MatchesPlayed++;
        }
        if (this.MatchSettings.Mode == Modes.DarkWorld)
        {
            this.DarkWorldState = new DarkWorldSessionState(this);
            if (!patch_SaveData.AdventureActive)
                SaveData.Instance.DarkWorld.Towers[this.MatchSettings.LevelSystem.ID.X].Attempts += 1UL;
        }
        this.TreasureSpawner = this.MatchSettings.LevelSystem.GetTreasureSpawner(this);
        Engine.Instance.Scene = new LevelLoaderXML(this);
    }
}