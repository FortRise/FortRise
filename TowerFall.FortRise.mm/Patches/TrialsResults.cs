using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TrialsResults : TrialsResults
{
    private long time;
    private Point levelID;
    private long oldBest;
    private bool newBestTime;
    private bool unlockGold;
    private bool unlockDiamond;
    private bool unlockDevTime;


    public patch_TrialsResults(Session session, long time, bool unlockGold, bool unlockDiamond, bool unlockDevTime, bool newBestTime, long oldBest) : base(session, time, unlockGold, unlockDiamond, unlockDevTime, newBestTime, oldBest)
    {
    }

    [MonoModLinkTo("TowerFall.HUD", "System.Void .ctor()")]
    [MonoModIgnore]
    public void thisctor() {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Session session, long time, bool unlockGold, bool unlockDiamond, bool unlockDevTime, bool newBestTime, long oldBest) 
    {
        thisctor();
        this.time = time;
        this.unlockGold = unlockGold;
        this.unlockDiamond = unlockDiamond;
        this.unlockDevTime = unlockDevTime;
        this.newBestTime = newBestTime;
        this.oldBest = oldBest;
        TrialsLevelData trialsLevelData = (session.MatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;
        this.levelID = trialsLevelData.ID;
        int y = this.levelID.Y;
        session.CurrentLevel.Add<HUDFade>(new HUDFade());
        if (time == 0L)
        {
            base.Add(new Coroutine(this.DeadResults()));
            return;
        }
        base.Add(new Coroutine(this.CompleteResults()));
    }

    [MonoModReplace]
    private IEnumerator CompleteResults()
    {
        yield return 20;
        Text text2 = new Text(TFGame.Font, "COMPLETE!", new Vector2(160f, 30f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        text2.Color = Calc.HexToColor("B6FF00");
        text2.Scale = Vector2.Zero;
        this.Add(text2);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 20, true);
        tween.OnUpdate = t => {
            text2.Scale = Vector2.One * 3f * tween.Eased;
        };
        this.Add(tween);
        Vector2 textStart = new Vector2(400f, 70f);
        Vector2 textEnd = new Vector2(160f, 70f);
        OutlineText text = new OutlineText(TFGame.Font, TrialsResults.GetTimeString(this.time), textStart, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        text.Color = Color.White;
        text.Scale = Vector2.One * 3f;
        this.Add(text);
        Alarm.Set(this, 40, () =>
        {
            Sounds.sfx_trainingStartLevelWhoosh2.Play(160f, 0.6f);
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 10, true);
            Tween tween2 = tween3;
            tween2.OnUpdate = t =>
            {
                text.Position = Vector2.Lerp(textStart, textEnd, t.Eased);
            };
            this.Add(tween3);
        }, Alarm.AlarmMode.Oneshot);

        Saver saver = new Saver(false);
        this.Level.Add<Saver>(saver);
        TrialsTimes trialsTimes = new TrialsTimes(new Vector2(160f, 130f), this.levelID, false, this.unlockGold, this.unlockDiamond, this.unlockDevTime, this.newBestTime, this.oldBest);
        this.Level.Add<TrialsTimes>(trialsTimes);
        yield return trialsTimes.Wait();
        yield return 40;
        saver.CanHandleError = true;
        while (!saver.Finished)
        {
            yield return 0;
        }
        this.Level.Add<PauseMenu>(new PauseMenu(this.Level, new Vector2(160f, 200f), PauseMenu.MenuType.TrialsComplete, -1));
        yield break;
    }

    [MonoModReplace]
    private IEnumerator DeadResults()
    {
        Sounds.sfx_trainingFailureTone.Play(160f, 1f);
        yield return 20;
        Text text = new Text(TFGame.Font, "FAILURE", new Vector2(160f, 60f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        text.Color = Color.Red;
        text.Scale = Vector2.Zero;
        this.Add(text);
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 20, true);
        tween.OnUpdate = t => {
            text.Scale = Vector2.One * 3f * tween.Eased;
        };
        tween.OnComplete = t =>
        {
            int num = 10;

            Alarm.Set(this, num, () => {
                Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, Ease.BounceOut, 60, true);
                Tween tween2 = tween3;
                
                tween2.OnUpdate = (t2) => {
                    text.Rotation = 0.17453292f * t2.Eased;
                };
                this.Add(tween3);
            }, 
            Alarm.AlarmMode.Oneshot);
        };
        this.Add(tween);
        Saver saver = new Saver(false, null);
        this.Level.Add<Saver>(saver);
        TrialsTimes trialsTimes = new TrialsTimes(new Vector2(160f, 130f), this.levelID, true, false, false, false, false, this.oldBest);
        this.Level.Add<TrialsTimes>(trialsTimes);
        yield return trialsTimes.Wait();
        yield return 40;
        saver.CanHandleError = true;
        while (!saver.Finished)
        {
            yield return 0;
        }
        this.Level.Add<PauseMenu>(new PauseMenu(this.Level, new Vector2(160f, 200f), PauseMenu.MenuType.TrialsFailure, -1));
        yield break;
    }


}