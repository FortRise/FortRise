using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.ArchivesTrialsPage")]
public class ArchivesTrialsPage : TowerFall.ArchivesTrialsPage
{
    private List<GraphicsComponent> graphics;
    private List<string> towerSets;
    private int currentIndex;
    private OutlineText titleSet;
    private Image upArrow;
    private Image downArrow;
    private SineWave arrowSine;

    [MonoModLinkTo("TowerFall.ArchivesPage", "System.Void .ctor(System.String)")]
    [MonoModIgnore]
    public void base_ctor(string title) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor()
    {
        graphics = [];

        towerSets = ["All", "TowerFall"];
        towerSets.AddRange(TowerRegistry.TrialsLevelSet);

        titleSet = new OutlineText(TFGame.Font, "ALL", new Vector2(0, -10), Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        Add(titleSet);

        upArrow = new Image(TFGame.MenuAtlas["portraits/arrow"]);
        upArrow.CenterOrigin();
        Add(upArrow);

        downArrow = new Image(TFGame.MenuAtlas["portraits/arrow"]);
        downArrow.CenterOrigin();
        Add(downArrow);

        var textWidth = TFGame.Font.MeasureString("ALL").X;

        upArrow.Position = new Vector2(textWidth + 10, -10);
        upArrow.Rotation = 90 * Calc.DEG_TO_RAD;
        downArrow.Position = new Vector2(-textWidth - 10, -10);
        downArrow.Rotation = 270 * Calc.DEG_TO_RAD;

        Add(arrowSine = new SineWave(120));

        base_ctor("TRIALS");

        Create();
    }

    public override void Update()
    {
        base.Update();
        if (!IsOnscreen)
        {
            return;
        }

        if (MenuInput.Down)
        {
            currentIndex += 1;
            if (currentIndex > towerSets.Count - 1)
            {
                currentIndex = towerSets.Count - 1;
                return;
            }

            this.Remove(graphics);
            graphics.Clear();

            Create(towerSets[currentIndex]);
            Sounds.ui_click.Play();
        }
        else if (MenuInput.Up)
        {
            currentIndex -= 1;
            if (currentIndex < 0)
            {
                currentIndex = 0;
                return;
            }

            this.Remove(graphics);
            graphics.Clear();

            Create(towerSets[currentIndex]);
            Sounds.ui_click.Play();
        }
    }

    public override void Render()
    {
        base.Render();

        upArrow.Position.Y = -10 + arrowSine.Value * 2f;
        downArrow.Position.Y = -10 - arrowSine.Value * 2f;

        upArrow.Color = currentIndex == towerSets.Count - 1 ? Color.Transparent : Color.White;
        downArrow.Color = currentIndex == 0 ? Color.Transparent : Color.White;
    }

    private void Create(string set = null)
    {
        int gold = 0;
        int diamond = 0;
        int devTime = 0;
        ulong attempts = 0;
        long bestTime = 0;
        int total = 0;

        bool hasPlayed = true;

        set ??= "All";

        if (set is "All" or "TowerFall")
        {
            var stats = set == "All" ? SaveData.Instance.Trials.Levels : SaveData.Instance.Trials.Levels[0..16];

            for (int x = 0; x < stats.Length; x += 1)
            {
                var level = stats[x];
                for (int y = 0; y < level.Length; y += 1)
                {
                    total += 1;
                    var stat = stats[x][y];
                    if (stat.UnlockedGold)
                    {
                        gold += 1;
                    }

                    if (stat.UnlockedDiamond)
                    {
                        diamond += 1;
                    }

                    if (stat.UnlockedDevTime)
                    {
                        devTime += 1;
                    }
                    attempts += stat.Attempts;
                    if (stat.BestTime == 0L)
                    {
                        hasPlayed = false;
                    }
                    else
                    {
                        bestTime = stat.BestTime;
                    }
                }
            }
        }
        else
        {
            var stats = TowerRegistry.TrialTowers
                .Where(x => x.Value.TowerSet == set)
                .Select(x => FortRiseModule.SaveData.AdventureTrials.Towers[x.Key]);

            foreach (var stat in stats)
            {
                total += 1;
                if (stat.UnlockedGold)
                {
                    gold += 1;
                }

                if (stat.UnlockedDiamond)
                {
                    diamond += 1;
                }

                if (stat.UnlockedDevTime)
                {
                    devTime += 1;
                }
                attempts += stat.Attempts;
                if (stat.BestTime == 0L)
                {
                    hasPlayed = false;
                }
                else
                {
                    bestTime = stat.BestTime;
                }
            }
        }

        int index = set.LastIndexOf('/');
        if (index != -1)
        {
            set = set[(index + 1)..];
        }

        var textWidth = TFGame.Font.MeasureString(set).X;

        titleSet.DrawText = set.ToUpperInvariant();
        upArrow.Position = new Vector2(textWidth + 10, -10);
        downArrow.Position = new Vector2(-textWidth - 10, -10);

        Sprite<int> awardSprite = TFGame.MenuSpriteData.GetSpriteInt("Gold");
        graphics.Add(awardSprite);

        OutlineText awardText = new OutlineText(TFGame.Font, gold.ToString() + " / " + total.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        };
        graphics.Add(awardText);

        float offset = awardSprite.Width + 10f + awardText.Width * awardText.Scale.X;
        awardSprite.Position = new Vector2(-offset / 2f + awardSprite.Width / 2f, 10f);
        awardText.Position = new Vector2(offset / 2f - awardText.Width / 2f * awardText.Scale.X, 10f);
        awardSprite = TFGame.MenuSpriteData.GetSpriteInt("Diamond");
        graphics.Add(awardSprite);

        awardText = new OutlineText(TFGame.Font, diamond.ToString() + " / " + total.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        };

        graphics.Add(awardText);

        offset = awardSprite.Width + 10f + awardText.Width * awardText.Scale.X;
        awardSprite.Position = new Vector2(-offset / 2f + awardSprite.Width / 2f, 40f);
        awardText.Position = new Vector2(offset / 2f - awardText.Width / 2f * awardText.Scale.X, 40f);
        awardSprite = TFGame.MenuSpriteData.GetSpriteInt("DevTime");
        graphics.Add(awardSprite);

        awardText = new OutlineText(TFGame.Font, devTime.ToString() + " / " + total.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        };
        graphics.Add(awardText);

        offset = awardSprite.Width + 10f + awardText.Width * awardText.Scale.X;
        awardSprite.Position = new Vector2(-offset / 2f + awardSprite.Width / 2f, 70f);
        awardText.Position = new Vector2(offset / 2f - awardText.Width / 2f * awardText.Scale.X, 70f);

        OutlineText attemptsText = new OutlineText(TFGame.Font, "TOTAL ATTEMPTS:", new Vector2(-60f, 100f), TowerFall.ArchivesQuestPage.SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        graphics.Add(attemptsText);
        graphics.Add(new OutlineText(TFGame.Font, attempts.ToString(), new Vector2(-60f, 112f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });

        string bestTimeText;
        if (hasPlayed)
        {
            bestTimeText = TrialsResults.GetTimeString(bestTime);
        }
        else
        {
            bestTimeText = "N / A";
        }

        attemptsText = new OutlineText(TFGame.Font, "TOTAL BEST TIME:", new Vector2(60f, 100f), TowerFall.ArchivesQuestPage.SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        graphics.Add(attemptsText);
        graphics.Add(new OutlineText(TFGame.Font, bestTimeText, new Vector2(60f, 112f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });

        if (set is "All" or "TowerFall")
        {
            Sprite<string> yellowGems = TFGame.MenuSpriteData.GetSpriteString("Gem5");
            if (SaveData.Instance.Unlocks.YellowGemsFound.Count > 0)
            {
                yellowGems.Play("on", false);
            }
            graphics.Add(yellowGems);

            awardText = new OutlineText(TFGame.Font, SaveData.Instance.Unlocks.YellowGemsFound.Count + " / " + 7, Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };
            graphics.Add(awardText);

            offset = yellowGems.Width + 10f + awardText.Width * awardText.Scale.X;
            yellowGems.Position = new Vector2(-offset / 2f + yellowGems.Width / 2f, 140f);
            awardText.Position = new Vector2(offset / 2f - awardText.Width / 2f * awardText.Scale.X, 140f);
        }

        this.Add(graphics);
    }
}
