using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_SwitchBlockControl : SwitchBlockControl
{
    private Session session;
    private Counter timer;


    public patch_SwitchBlockControl(Session session) : base(session)
    {
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void .ctor(System.Int32)")]
    [MonoModIgnore]
    public void thisctor(int layerIndex = 0) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Session session) 
    {
        thisctor();
        this.Visible = false;
        this.session = session;
        this.timer = new Counter();
        if (session.MatchSettings.Mode == Modes.Trials)
        {
            this.Active = false;
            var level = (MainMenu.TrialsMatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;
            this.timer.Set(level.SwitchBlockTimer);
            return;
        }
        this.timer.Set(300);
    }
}