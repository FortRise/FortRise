using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.VersusRoundResults")]
public class VersusRoundResults : TowerFall.VersusRoundResults
{
    private Session session;
    private float startFrames;
    private Sprite<int> prototypePoint;
    private int[] currentScores;
    private Sprite<int>[][] points;
    private Image[] crowns;
    private Text[] overflowText;
    private bool finished;
    private bool replaySaved;
    private MenuButtonGuide confirmGuide;
    private MenuButtonGuide replayGuide;
    private MenuButtonGuide saveReplayGuide;

    public VersusRoundResults(Session session, List<EventLog> events) : base(session, events)
    {
    }

    [MonoModReplace]
    private void PlayGainSound()
    {
        if (session.MatchSettings.Mode == Modes.HeadHunters)
        {
            Sounds.sfx_multiSkullEarned.Play(160f, 1f);
            return;
        }
        if ((session.MatchSettings as patch_MatchSettings).IsCustom) 
        {
            var gameMode = (session.MatchSettings as patch_MatchSettings).CustomVersusGameMode;
            if (gameMode != null) 
            {
                gameMode.OverrideEarnedCoinSFX(session).Play(160f, 1f);
                return;
            }
        }
        Sounds.sfx_multiCoinEarned.Play(160f, 1f);
    }

    [MonoModReplace]
    private void LosePoint(Sprite<int> point)
    {
        point.Stop();
        point.CurrentFrame = 0;
        point.Color = DeathSkull.SuicideColor * 0.35f;
        Wiggler wiggler = Wiggler.Create(30, 3f, null, v =>
        {
            point.Scale = Vector2.One * (1f + v * 0.3f);
        }, true, true);
        Add(wiggler);

        if ((session.MatchSettings as patch_MatchSettings).IsCustom) 
        {
            var gameMode = (session.MatchSettings as patch_MatchSettings).CustomVersusGameMode;
            if (gameMode != null) 
            {
                gameMode.OverrideLoseCoinSFX(session).Play(160f, 1f);
                return;
            }
        }
        Sounds.sfx_multiSkullNegative.Play(160f, 1f);
    }

    [MonoModReplace]
    private void LosePointOverflow(Text overflowText, int totalScore)
    {
        if (totalScore <= session.MatchSettings.GoalScore)
        {
            overflowText.Color = DeathSkull.SuicideColor;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 30, true);
            tween.OnUpdate = t =>
            {
                overflowText.Scale = Vector2.One * (1f - t.Eased);
            };
            Add(tween);
        }
        else
        {
            overflowText.Color = DeathSkull.SuicideColor;
            overflowText.DrawText = "+" + (totalScore - session.MatchSettings.GoalScore);
            Wiggler wiggler = Wiggler.Create(30, 3f, null, v =>
            {
                overflowText.Scale = Vector2.One * (1f + 0.3f * v);
            }, true, true);
            Add(wiggler);
        }
        if ((session.MatchSettings as patch_MatchSettings).IsCustom) 
        {
            var gameMode = (session.MatchSettings as patch_MatchSettings).CustomVersusGameMode;
            if (gameMode != null) 
            {
                gameMode.OverrideLoseCoinSFX(session).Play(160f, 1f);
                return;
            }
        }
        Sounds.sfx_multiSkullNegative.Play(160f, 1f);
    }

    private Sprite<int> GetCustomSpriteOrNot() 
    {
        if (patch_MainMenu.VersusMatchSettings.IsCustom) 
        {
            var gameMode = patch_MainMenu.VersusMatchSettings.CustomVersusGameMode;
            if (gameMode != null) 
            {
                return gameMode.OverrideCoinSprite(session);
            }
        }
        return VersusCoinButton.GetCoinSprite();
    }

    private SFX GetCustomSoundOrNot() 
    {
        if (patch_MainMenu.VersusMatchSettings.IsCustom) 
        {
            var gameMode = patch_MainMenu.VersusMatchSettings.CustomVersusGameMode;
            if (gameMode != null) 
            {
                return gameMode.OverrideEarnedCoinSFX(session);
            }
        }
        return Sounds.sfx_multiCoinEarned;
    }

    private int GetCustomOffsetOrNot() 
    {
        if (patch_MainMenu.VersusMatchSettings.IsCustom) 
        {
            var gameMode = patch_MainMenu.VersusMatchSettings.CustomVersusGameMode;
            if (gameMode != null) 
            {
                return gameMode.OverrideCoinOffset(session);
            }
        }
        return 10;
    }

    [MonoModReplace]
    private IEnumerator Sequence(List<EventLog> events)
    {
        startFrames = Level.FrameCounter;
        Func<Sprite<int>> spriteGetter;
        int coinSep;
        if (session.MatchSettings.Mode == Modes.HeadHunters)
        {
            spriteGetter = new Func<Sprite<int>>(DeathSkull.GetSprite);
            coinSep = 12;
        }
        else
        {
            spriteGetter = new Func<Sprite<int>>(GetCustomSpriteOrNot);
            coinSep = GetCustomOffsetOrNot();
        }
        int playerTextsAcross = session.MatchSettings.GetMaxTeamSize();
        int width = session.MatchSettings.GoalScore * coinSep + 25 * playerTextsAcross;
        int coinDrawX = 160 - width / 2 + 25 * playerTextsAcross;

        prototypePoint = spriteGetter.Invoke();
        prototypePoint.Visible = false;
        prototypePoint.Play(0, false);
        Add(prototypePoint);
        currentScores = new int[session.Scores.Length];
        points = new Sprite<int>[session.Scores.Length][];
        overflowText = new Text[session.Scores.Length];

        for (int i = 0; i < session.Scores.Length; i++)
        {
            if (session.MatchSettings.TeamMode || TFGame.Players[i])
            {
                int oldScore = session.GetOldScore(i);
                currentScores[i] = oldScore;
                points[i] = new Sprite<int>[session.MatchSettings.GoalScore];
                for (int j = 0; j < session.MatchSettings.GoalScore; j++)
                {
                    points[i][j] = spriteGetter.Invoke();
                    points[i][j].Position = new Vector2(coinDrawX + j * coinSep, 80 + i * 20);
                    Add(points[i][j]);
                    if (j < oldScore)
                    {
                        points[i][j].Play(0, false);
                    }
                    else
                    {
                        points[i][j].Stop();
                        points[i][j].Color = Color.White * 0.35f;
                    }
                }
                overflowText[i] = new Text(TFGame.Font, "", new Vector2(coinDrawX + session.MatchSettings.GoalScore * coinSep + 2, 80 + i * 20), Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
                Add(overflowText[i]);
                if (oldScore > session.MatchSettings.GoalScore)
                {
                    overflowText[i].DrawText = "+" + (oldScore - session.MatchSettings.GoalScore);
                }
            }
        }

        int num = coinDrawX - 12;
        int[] array = new int[2];
        for (int i = 3; i >= 0; i--)
        {
            if (TFGame.Players[i])
            {
                string text = "P" + (i + 1).ToString();
                Vector2 vector;
                if (session.MatchSettings.TeamMode)
                {
                    int scoreIndex = session.GetScoreIndex(i);
                    vector = new Vector2(num - array[scoreIndex] * 25, 81 + scoreIndex * 20);
                    array[scoreIndex]++;
                }
                else
                {
                    vector = new Vector2(num, 81 + i * 20);
                }
                Add(new Text(TFGame.Font, text, vector, ArcherData.GetColorA(i, Allegiance.Neutral), Text.HorizontalAlign.Right, Text.VerticalAlign.Center)
                {
                    Scale = Vector2.One * 2f
                });
            }
        }

        crowns = new Image[session.Scores.Length];
        for (int i = 0; i < session.Scores.Length; i++)
        {
            crowns[i] = new Image(TFGame.Atlas["versus/crown"]);
            crowns[i].CenterOrigin();
            if (session.MatchSettings.TeamMode)
            {
                crowns[i].Position = new Vector2(num - 25 * array[i] - crowns[i].Width / 2f, 80 + i * 20);
            }
            else
            {
                crowns[i].Position = new Vector2(num - 25 - crowns[i].Width / 2f, 80 + i * 20);
            }
            if (session.TeamHadCrown(i))
            {
                crowns[i].Scale = Vector2.One;
            }
            else
            {
                crowns[i].Scale = Vector2.Zero;
            }
            Add(crowns[i]);
        }

        yield return 10;

        if (MatchResults != null)
        {
            int winner = session.GetWinner();
            if (session.MatchSettings.TeamMode)
            {
                ArcherData.Teams[winner].PlayVictoryMusic();
            }
            else
            {
                ArcherData.Get(TFGame.Characters[winner], TFGame.AltSelect[winner]).PlayVictoryMusic();
            }
        }
        else if (session.IsInOvertime && !session.WasInOvertime)
        {
            Music.Stop();
        }

        yield return 30;

        if (events != null)
        {
            // foreach loop breaks the coroutine
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                yield return e.Sequence(this);
            }
        }

        yield return 10;

        finished = true;
        Add(confirmGuide = new MenuButtonGuide(0, MenuButtonGuide.ButtonModes.Confirm, "CONTINUE"));
        Add(replayGuide = new MenuButtonGuide(1, MenuButtonGuide.ButtonModes.Alt, "REPLAY"));
        Add(saveReplayGuide = new MenuButtonGuide(2, MenuButtonGuide.ButtonModes.SaveReplay, "SAVE REPLAY"));
        if (SaveData.Instance.Options.ReplayMode == Options.ReplayModes.Off)
        {
            replayGuide.Clear();
        }
        if (replaySaved || SaveData.Instance.Options.ReplayMode == Options.ReplayModes.Off)
        {
            saveReplayGuide.Clear();
        }
        else if (MenuInput.SaveReplayCheck)
        {
            SaveReplay();
        }
    }

    [MonoModIgnore]
    private extern void SaveReplay();
}
