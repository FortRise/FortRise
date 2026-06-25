using System;
using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.ArchivesQuestPage")]
public class ArchivesQuestPage : TowerFall.ArchivesQuestPage
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
        towerSets.AddRange(TowerRegistry.QuestLevelSets);

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

        base_ctor("QUEST");

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
        int questCount;
        TowerFall.Patching.QuestTowerStats[] stats;

        set ??= "All";

        if (set == "All")
        {
            questCount = SaveData.Instance.Quest.Towers.Length;
            stats = SaveData.Instance.Quest.Towers as TowerFall.Patching.QuestTowerStats[];
        }
        else if (set is "TowerFall")
        {
            questCount = GameData.QuestLevels.Count(x => x.TowerSet == set);
            stats = SaveData.Instance.Quest.Towers[0..14] as TowerFall.Patching.QuestTowerStats[];
        }
        else
        {
            questCount = GameData.QuestLevels.Count(x => x.TowerSet == set);
            stats = [.. TowerRegistry.QuestTowers.Where(x => x.Value.TowerSet == set).Select(x => FortRiseModule.SaveData.AdventureQuest.Towers[x.Key])];
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

        int totalWhiteSkulls = stats.Count(x => x.CompletedNormal);
        int totalRedSkulls = stats.Count(x => x.CompletedHardcore);
        int totalGoldSkulls = stats.Count(x => x.CompletedNoDeaths);

        ulong totalAttempts = (ulong)stats.Sum(x => (decimal)x.TotalAttempts);
        ulong totalDeaths = (ulong)stats.Sum(x => (decimal)x.TotalDeaths);

        Image skullImage = new Image(TFGame.MenuAtlas["questResults/bigWhiteSkull"]);
        skullImage.CenterOrigin();
        graphics.Add(skullImage);

        OutlineText totalTextCount = new OutlineText(TFGame.Font, $"{totalWhiteSkulls} / {questCount}", Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        totalTextCount.Scale = Vector2.One * 2f;
        graphics.Add(totalTextCount);

        float width = skullImage.Width + 10f + totalTextCount.Width * totalTextCount.Scale.X;
        skullImage.Position = new Vector2(-width / 2f + skullImage.Width / 2f, 20f);
        totalTextCount.Position = new Vector2(width / 2f - totalTextCount.Width / 2f * totalTextCount.Scale.X, 20f);
        skullImage = new Image(TFGame.MenuAtlas["questResults/bigRedSkull"]);
        skullImage.CenterOrigin();
        graphics.Add(skullImage);

        totalTextCount = new OutlineText(TFGame.Font, $"{totalRedSkulls} / {questCount}", Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        totalTextCount.Scale = Vector2.One * 2f;
        graphics.Add(totalTextCount);

        width = skullImage.Width + 10f + totalTextCount.Width * totalTextCount.Scale.X;
        skullImage.Position = new Vector2(-width / 2f + skullImage.Width / 2f, 50f);
        totalTextCount.Position = new Vector2(width / 2f - totalTextCount.Width / 2f * totalTextCount.Scale.X, 50f);
        skullImage = new Image(TFGame.MenuAtlas["questResults/bigGoldSkull"]);
        skullImage.CenterOrigin();
        graphics.Add(skullImage);

        totalTextCount = new OutlineText(TFGame.Font, $"{totalGoldSkulls} / {questCount}", Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        totalTextCount.Scale = Vector2.One * 2f;
        graphics.Add(totalTextCount);

        width = skullImage.Width + 10f + totalTextCount.Width * totalTextCount.Scale.X;
        skullImage.Position = new Vector2(-width / 2f + skullImage.Width / 2f, 80f);
        totalTextCount.Position = new Vector2(width / 2f - totalTextCount.Width / 2f * totalTextCount.Scale.X, 80f);

        OutlineText totalText = new OutlineText(TFGame.Font, "TOTAL ATTEMPTS:", new Vector2(-60f, 120f), SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        graphics.Add(totalText);
        graphics.Add(new OutlineText(TFGame.Font, $"{totalAttempts}", new Vector2(-60f, 132f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });
        totalText = new OutlineText(TFGame.Font, "TOTAL DEATHS:", new Vector2(60f, 120f), SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center);
        graphics.Add(totalText);
        graphics.Add(new OutlineText(TFGame.Font, $"{totalDeaths}", new Vector2(60f, 132f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });

        this.Add(graphics);
    }
}
