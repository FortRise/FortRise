using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TrialsTimes : TrialsTimes
{
    private Action<Tween> cleanUp;
    private bool tweenOut;
    private bool finished;

    public patch_TrialsTimes(Vector2 position, Point levelID, bool failureSounds, bool unlockGold, bool unlockDiamond, bool unlockDevTime, bool newBestTime, long oldBest) : base(position, levelID, failureSounds, unlockGold, unlockDiamond, unlockDevTime, newBestTime, oldBest)
    {
    }

    [MonoModReplace]
    private IEnumerator Sequence(Point levelID, bool failureSounds, bool unlockGold, bool unlockDiamond, bool unlockDevTime, bool newBestTime, long oldBest)
    {
        bool unlockedGold;
        bool unlockedDiamond;
        bool unlockedDevTime;
        long bestTime;
        TimeSpan[] goals;
        var tower = (Level.Session.MatchSettings.LevelSystem as TrialsLevelSystem).TrialsLevelData;

        if (tower.GetLevelSet() == "TowerFall") 
        {
            TrialsLevelStats stats = SaveData.Instance.Trials.Levels[levelID.X][levelID.Y];
            unlockedGold = stats.UnlockedGold;
            unlockedDiamond = stats.UnlockedDiamond;
            unlockedDevTime = stats.UnlockedDevTime;
            bestTime = stats.BestTime;
            goals = tower.Goals;
        }
        else 
        {
            var stats = (tower as patch_TrialsLevelData).Stats;
            unlockedGold = stats.UnlockedGold;
            unlockedDiamond = stats.UnlockedDiamond;
            unlockedDevTime = stats.UnlockedDevTime;
            bestTime = stats.BestTime;
            goals = tower.Goals;
        }


        int goalsX = ((bestTime == 0L) ? 0 : (-60));
        Image goldCheck = null;
        Image diamondCheck = null;
        Sprite<int> devTimeMedal = null;

        Vector2 targetPos3 = new Vector2((float)goalsX, -15f);
        Vector2 startPos3 = targetPos3 + new Vector2(-320f, 0f);
        var goldText = new OutlineText(TFGame.Font, TrialsResults.GetTimeString(goals[0]), startPos3, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        goldText.Scale = Vector2.One * 2f;
        Add(goldText);
        var goldMedal = TFGame.MenuSpriteData.GetSpriteInt("Gold");
        goldMedal.CenterOrigin();
        goldMedal.Play(0, false);
        goldMedal.Position = startPos3;
        Add(goldMedal);
        float num = goldText.Width * 2f + goldMedal.Width + 15f;
        goldText.Origin.X = goldText.Width * 2f - num / 2f;
        goldMedal.Origin.X = num / 2f;
        if (unlockedGold || unlockGold)
        {
            goldCheck = new Image(TFGame.MenuAtlas["trials/check"], null);
            goldCheck.Origin = goldMedal.Origin;
            goldCheck.Position = goldMedal.Position;
            Add(goldCheck);
            if (unlockGold)
            {
                goldCheck.Visible = false;
            }
        }
        if (failureSounds)
        {
            Sounds.sfx_trainingFailureWhoosh1.Play(160f, 0.6f);
        }
        else
        {
            Sounds.sfx_trainingStartLevelWhoosh1.Play(160f, 0.6f);
        }
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween.OnUpdate = t => {
            goldMedal.Position = (goldText.Position = Vector2.Lerp(startPos3, targetPos3, t.Eased));
            if (goldCheck)
            {
                goldCheck.Position = goldMedal.Position;
            }
        };
        this.Add(tween);
        this.cleanUp += t => {
            goldMedal.Position = (goldText.Position = Vector2.Lerp(targetPos3, startPos3, t.Eased));
            if (goldCheck)
            {
                goldCheck.Position = goldMedal.Position;
            }
        };
        yield return 10;
        Vector2 targetPos = new Vector2((float)goalsX, 15f);
        Vector2 startPos = targetPos + new Vector2(-320f, 0f);
        var diamondText = new OutlineText(TFGame.Font, TrialsResults.GetTimeString(goals[1]), startPos, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        diamondText.Scale = Vector2.One * 2f;
        Add(diamondText);

        var diamondMedal = TFGame.MenuSpriteData.GetSpriteInt("Diamond");
        diamondMedal.CenterOrigin();
        diamondMedal.Play(0, false);
        diamondMedal.Position = startPos;
        Add(diamondMedal);

        float num2 = diamondText.Width * 2f + diamondMedal.Width + 15f;
        diamondText.Origin.X = diamondText.Width * 2f - num2 / 2f;
        diamondMedal.Origin.X = num2 / 2f;
        if (unlockedDiamond || unlockDiamond)
        {
            diamondCheck = new Image(TFGame.MenuAtlas["trials/check"], null);
            diamondCheck.Origin = diamondMedal.Origin;
            diamondCheck.Position = diamondMedal.Position;
            Add(diamondCheck);
            if (unlockDiamond)
            {
                diamondCheck.Visible = false;
            }
        }
        if (failureSounds)
        {
            Sounds.sfx_trainingFailureWhoosh1.Play(160f, 0.6f);
        }
        else
        {
            Sounds.sfx_trainingStartLevelWhoosh1.Play(160f, 0.6f);
        }
        Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween2.OnUpdate = t =>
        {
            diamondMedal.Position = (diamondText.Position = Vector2.Lerp(startPos, targetPos, t.Eased));
            if (diamondCheck)
            {
                diamondCheck.Position = diamondMedal.Position;
            }
        };
        this.Add(tween2);
        this.cleanUp += t =>
        {
            diamondMedal.Position = (diamondText.Position = Vector2.Lerp(targetPos, startPos, t.Eased));
            if (diamondCheck)
            {
                diamondCheck.Position = diamondMedal.Position;
            }
        };
        yield return 10;
        if (unlockedDevTime || unlockDevTime)
        {
            Vector2 targetPos2 = new Vector2(60f, 15f);
            Vector2 startPos2 = targetPos2 + new Vector2(320f, 0f);
            devTimeMedal = TFGame.MenuSpriteData.GetSpriteInt("DevTime");
            devTimeMedal.CenterOrigin();
            devTimeMedal.Play(0, false);
            devTimeMedal.Position = startPos2;
            if (unlockDevTime)
            {
                devTimeMedal.Visible = false;
            }
            Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
            tween3.OnUpdate = t =>
            {
                devTimeMedal.Position = Vector2.Lerp(startPos2, targetPos2, t.Eased);
            };
            Add(tween3);
            cleanUp += t =>
            {
                devTimeMedal.Position = Vector2.Lerp(targetPos2, startPos2, t.Eased);
            };
        }
        if (bestTime != 0L)
        {
            var targetPosBestTime = new Vector2(60f, 0f);
            var startPosBestTime = targetPosBestTime + new Vector2(320f, 0f);
            var titleOffset = new Vector2(0f, -14f);
            string text;
            if (newBestTime)
            {
                if (oldBest == 0L)
                {
                    text = "---";
                }
                else
                {
                    text = TrialsResults.GetTimeString(oldBest);
                }
            }
            else
            {
                text = TrialsResults.GetTimeString(bestTime);
            }
            var bestText = new OutlineText(TFGame.Font, text, startPosBestTime, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
            bestText.Scale = Vector2.One * 3f;
            Add(bestText);
            var bestTitle = new OutlineText(TFGame.Font, "BEST:", startPosBestTime + titleOffset, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
            Add(bestTitle);
            if (devTimeMedal != null)
            {
                this.Add(devTimeMedal);
            }
            if (failureSounds)
            {
                Sounds.sfx_trainingFailureWhoosh2.Play(160f, 0.6f);
            }
            else
            {
                Sounds.sfx_trainingStartLevelWhoosh2.Play(160f, 0.6f);
            }
            Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
            tween4.OnUpdate = delegate(Tween t)
            {
                bestText.Position = Vector2.Lerp(startPosBestTime, targetPosBestTime, t.Eased);
                bestTitle.Position = Vector2.Lerp(startPosBestTime + titleOffset, targetPosBestTime + titleOffset, t.Eased);
            };
            this.Add(tween4);
            this.cleanUp += t =>
            {
                bestText.Position = Vector2.Lerp(targetPosBestTime, startPosBestTime, t.Eased);
                bestTitle.Position = Vector2.Lerp(targetPosBestTime + titleOffset, startPosBestTime + titleOffset, t.Eased);
            };
            yield return 30;
            if (newBestTime)
            {
                yield return 30;
                Sounds.sfx_personal.Play(160f, 1f);
                bestText.DrawText = TrialsResults.GetTimeString(bestTime);
                bestText.Color = (bestTitle.Color = Calc.HexToColor("FFDE3D"));
                Wiggler wiggler = Wiggler.Create(30, 5f, null, v =>
                {
                    bestText.Scale = Vector2.One * (3f + 0.4f * v);
                    bestTitle.Scale = Vector2.One * (1f + 0.2f * v);
                }, true, true);
                this.Add(wiggler);
                yield return 40;
            }
        }
        if (unlockGold)
        {
            Sounds.sfx_gold.Play(160f, 1f);
            Tween tween5 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 10, true);
            tween5.OnUpdate = t =>
            {
                goldCheck.Scale = Vector2.One * MathHelper.Lerp(2f, 1f, t.Eased);
                goldCheck.Color = Color.White * MathHelper.Lerp(0f, 1f, t.Eased);
            };
            this.Add(tween5);
            goldCheck.Visible = true;
            Wiggler wiggler2 = Wiggler.Create(30, 5f, null, v =>
            {
                goldCheck.Scale = (goldMedal.Scale = Vector2.One * (1f + 0.2f * v));
                goldText.Scale = Vector2.One * (2f + 0.4f * v);
            }, true, true);
            this.Add(wiggler2);
            yield return 30;
        }
        if (unlockDiamond)
        {
            Sounds.sfx_diamond.Play(160f, 1f);
            Tween tween6 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 10, true);
            tween6.OnUpdate = t =>
            {
                diamondCheck.Scale = Vector2.One * MathHelper.Lerp(2f, 1f, t.Eased);
                diamondCheck.Color = Color.White * MathHelper.Lerp(0f, 1f, t.Eased);
            };
            this.Add(tween6);
            diamondCheck.Visible = true;
            Wiggler wiggler3 = Wiggler.Create(30, 5f, null, v =>
            {
                diamondCheck.Scale = (diamondMedal.Scale = Vector2.One * (1f + 0.2f * v));
                diamondText.Scale = Vector2.One * (2f + 0.4f * v);
            }, true, true);
            this.Add(wiggler3);
            yield return 30;
        }
        if (unlockDevTime)
        {
            Sounds.sfx_devTimeGem.Play(160f, 1f);
            devTimeMedal.Color = Color.Transparent;
            devTimeMedal.Visible = true;
            Tween tween7 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 60, true);
            tween7.OnUpdate = delegate(Tween t)
            {
                devTimeMedal.Color = Color.Lerp(Color.Transparent, Color.White, t.Eased);
                devTimeMedal.Scale = Vector2.One * MathHelper.Lerp(2f, 1f, t.Eased);
            };
            this.Add(tween7);
            yield return 60;
        }
        this.finished = true;
        while (!this.tweenOut)
        {
            yield return 0;
        }
        Tween tween8 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 20, true);
        tween8.OnUpdate = this.cleanUp;
        this.Add(tween8);
        yield return tween8.Wait();
        this.RemoveSelf();
        yield break;
    }
}