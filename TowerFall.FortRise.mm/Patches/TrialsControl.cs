using System;
using System.Collections;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TrialsControl : TrialsControl 
{
    private Image[] images;
    private int drawing;
    private Text text;
    private Sprite<int> nextImage;
    private Text nextText;
    private Action<Tween> onTweenOut;
    private float flashAlpha;
    private TimeSpan time;


    [MonoModLinkTo("TowerFall.HUD", "System.Void Added()")]
    [MonoModIgnore]
    public void base_Added() 
    {
        base.Added();
    }

    [MonoModReplace]
    public override void Added()
    {
        base_Added();
        this.drawing = base.Level[GameTags.Dummy].Count;
        this.images = new Image[this.drawing];
        for (int i = 0; i < this.images.Length; i++)
        {
            int start = 518 - i * 20;
            int target = 308 - i * 20;
            Image img = new Image(TFGame.Atlas["dummy/hitCounter"], new Rectangle?(new Rectangle(0, 0, 28, 28)));
            img.CenterOrigin();
            img.Position = new Vector2((float)start, 12f);
            base.Add(img);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
            tween.OnUpdate = delegate(Tween t)
            {
                img.X = MathHelper.Lerp((float)start, (float)target, t.Eased);
            };
            base.Add(tween);
            this.images[i] = img;
        }
        this.text = new Text(TFGame.Font, "0.000", new Vector2(-160f, 5f), Text.HorizontalAlign.Left, Text.VerticalAlign.Top);
        this.text.Scale = Vector2.One * 2f;
        base.Add(this.text);
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween2.OnUpdate = delegate(Tween t)
        {
            this.text.X = MathHelper.Lerp(-160f, 5f, t.Eased);
        };
        base.Add(tween2);

        this.onTweenOut = delegate(Tween t)
        {
            this.text.X = MathHelper.Lerp(5f, -160f, t.Eased);
        };

        int nextGoal;
        long bestTime;
        Sprite<int> smallAward;
        TrialsLevelData trialsLevelData = (base.Level.Session.MatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;
        if (trialsLevelData.GetLevelSet() == "TowerFall") 
        {
            var trialsLevelStats = SaveData.Instance.Trials.Levels[trialsLevelData.ID.X][trialsLevelData.ID.Y];
            nextGoal = trialsLevelStats.NextGoal;
            bestTime = trialsLevelStats.BestTime;
            smallAward = trialsLevelStats.GetNextSmallAwardIcon();
        }
        else 
        {
            var trialsLevelStats = FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID());
            nextGoal = trialsLevelStats.NextGoal;
            bestTime = trialsLevelStats.BestTime;
            smallAward = trialsLevelStats.GetNextSmallAwardIcon();
        }
        if (nextGoal == -1)
        {
            this.nextText = new Text(TFGame.Font, "BEST: " + TrialsResults.GetTimeString(bestTime), new Vector2(-160f, 22f), Text.HorizontalAlign.Left, Text.VerticalAlign.Center);
            base.Add(this.nextText);
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
            tween3.OnUpdate = delegate(Tween t)
            {
                this.nextText.X = MathHelper.Lerp(-160f, 5f, t.Eased);
            };
            base.Add(tween3);
            this.onTweenOut = (Action<Tween>)Delegate.Combine(this.onTweenOut, new Action<Tween>(delegate(Tween t)
            {
                this.nextText.X = MathHelper.Lerp(5f, -160f, t.Eased);
            }));
            return;
        }
        this.nextText = new Text(TFGame.Font, TrialsResults.GetTimeString(trialsLevelData.Goals[nextGoal]), new Vector2(-160f, 22f), Text.HorizontalAlign.Left, Text.VerticalAlign.Center);
        base.Add(this.nextText);
        this.nextImage = smallAward;
        this.nextImage.Play(0, false);
        this.nextImage.Position = new Vector2(-162f, 21f);
        base.Add(this.nextImage);
        Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween4.OnUpdate = delegate(Tween t)
        {
            this.nextText.X = MathHelper.Lerp(-160f, 15f, t.Eased);
            this.nextImage.X = MathHelper.Lerp(-168f, 7f, t.Eased);
        };
        base.Add(tween4);
        this.onTweenOut = (Action<Tween>)Delegate.Combine(this.onTweenOut, new Action<Tween>(delegate(Tween t)
        {
            this.nextText.X = MathHelper.Lerp(15f, -160f, t.Eased);
            this.nextImage.X = MathHelper.Lerp(7f, -168f, t.Eased);
        }));
    }

    [MonoModReplace]
    private IEnumerator WinSequence()
    {
        TrialsLevelData trialsLevelData = (this.Level.Session.MatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;

        long oldBest;
        bool oldUnlockGold;
        bool oldUnlockDiamond;
        bool oldUnlockDevTime;
        Point id = trialsLevelData.ID;

        if (trialsLevelData.IsOfficialLevelSet())
        {
            TrialsLevelStats trialsLevelStats = SaveData.Instance.Trials.Levels[id.X][id.Y];
            oldBest = trialsLevelStats.BestTime;
            oldUnlockGold = trialsLevelStats.UnlockedGold;
            oldUnlockDiamond = trialsLevelStats.UnlockedDiamond;
            oldUnlockDevTime = trialsLevelStats.UnlockedDevTime;
        }
        else
        {
            var trialsLevelStats = FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID());
            oldBest = trialsLevelStats.BestTime;
            oldUnlockGold = trialsLevelStats.UnlockedGold;
            oldUnlockDiamond = trialsLevelStats.UnlockedDiamond;
            oldUnlockDevTime = trialsLevelStats.UnlockedDevTime;
        }

        long currentTime = this.time.Ticks;
        bool newBestTime = false;
        bool unlockGold = false;
        bool unlockDiamond = false;
        bool unlockDevTime = false;
        this.flashAlpha = 1f;
        if (this.time <= trialsLevelData.Goals[2])
        {
            Sounds.sfx_devTimeFinalDummy.Play(160f, 1f);
            Music.Stop();
        }
        else
        {
            Sounds.sfx_trainingSuccess.Play(160f, 1f);
        }
        if (oldBest == 0L || currentTime < oldBest)
        {
            newBestTime = true;
            if (trialsLevelData.IsOfficialLevelSet())
            {
                SaveData.Instance.Trials.Levels[id.X][id.Y].BestTime = currentTime;
            }
            else
            {
                FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID()).BestTime = currentTime;
            }
            if (!oldUnlockGold && this.time <= trialsLevelData.Goals[0])
            {
                unlockGold = true;
                if (trialsLevelData.IsOfficialLevelSet())
                {
                    SaveData.Instance.Trials.Levels[id.X][id.Y].UnlockedGold = true;
                }
                else
                {
                    FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID()).UnlockedGold = true;
                }
            }
            if (!oldUnlockDiamond && this.time <= trialsLevelData.Goals[1])
            {
                unlockDiamond = true;
                if (trialsLevelData.IsOfficialLevelSet())
                {
                    SaveData.Instance.Trials.Levels[id.X][id.Y].UnlockedDiamond = true;
                }
                else
                {
                    FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID()).UnlockedDiamond = true;
                }
            }
            if (!oldUnlockDevTime && this.time <= trialsLevelData.Goals[2])
            {
                unlockDevTime = true;
                if (trialsLevelData.IsOfficialLevelSet())
                {
                    SaveData.Instance.Trials.Levels[id.X][id.Y].UnlockedDevTime = true;
                }
                else
                {
                    FortRiseModule.SaveData.AdventureTrials.AddOrGet(trialsLevelData.GetLevelID()).UnlockedDevTime = true;
                }
            }
        }
        yield return 40;
        int num;
        for (int i = this.images.Length - 1; i >= 0; i = num - 1)
        {
            Image img = this.images[i];
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 40, true);
            tween.OnUpdate = delegate(Tween t)
            {
                img.Scale = Vector2.One * MathHelper.Lerp(0.6f, 1f, t.Eased);
            };
            this.Add(tween);
            yield return 6;
            num = i;
        }
        yield return 30;
        this.TweenOut();
        yield return 30;
        this.Level.Add<TrialsResults>(new TrialsResults(this.Level.Session, currentTime, unlockGold, unlockDiamond, unlockDevTime, newBestTime, oldBest));
        yield break;
    }

    [MonoModIgnore]
    private extern void TweenOut();
}