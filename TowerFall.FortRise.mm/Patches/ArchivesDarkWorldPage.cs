using System;
using System.Collections.Generic;
using System.Linq;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall.Editor;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.ArchivesDarkWorldPage")]
public class ArchivesDarkWorldPage : TowerFall.ArchivesDarkWorldPage
{
    private List<GraphicsComponent> graphics;
    private List<string> towerSets;
    private int currentIndex;

    [MonoModLinkTo("TowerFall.ArchivesPage", "System.Void .ctor(System.String)")]
    [MonoModIgnore]
    public void base_ctor(string title) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor()
    {
        graphics = [];

        towerSets = ["All", "TowerFall"];
        towerSets.AddRange(TowerRegistry.DarkWorldLevelSets);


        base_ctor("DARK WORLD");

        Create();
    }

    public override void Update()
    {
        base.Update();
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

    private void Create(string set = null)
    {
        int darkWorldCount;
        TowerFall.Patching.DarkWorldTowerStats[] stats;

        if (set is null || set == "All")
        {
            darkWorldCount = SaveData.Instance.DarkWorld.Towers.Length;
            stats = SaveData.Instance.DarkWorld.Towers as TowerFall.Patching.DarkWorldTowerStats[];
        }
        else if (set is "TowerFall")
        {
            darkWorldCount = GameData.DarkWorldTowers.Count(x => x.TowerSet == set);
            stats = SaveData.Instance.DarkWorld.Towers[0..5] as TowerFall.Patching.DarkWorldTowerStats[];
        }
        else
        {
            darkWorldCount = GameData.DarkWorldTowers.Count(x => x.TowerSet == set);
            stats = [.. TowerRegistry.DarkWorldTowers.Where(x => x.Value.TowerSet == set).Select(x => FortRiseModule.SaveData.AdventureWorld.Towers[x.Key])];
        }

        if (darkWorldCount <= 5)
        {
            float posX = (20f * darkWorldCount + 4f * (darkWorldCount - 1)) / -2f;

            for (int i = 0; i < darkWorldCount; i++)
            {
                Image image = new Image(TFGame.MenuAtlas["questResults/bigWhiteSkull"])
                {
                    X = posX + i * 4f + 20f * (i + 0.5f),
                    Y = 5f,
                    Color = stats[i].CompletedNormal ? Color.White : Color.Black
                };

                image.CenterOrigin();
                graphics.Add(image);
            }
            for (int i = 0; i < darkWorldCount; i++)
            {
                Image image = new Image(TFGame.MenuAtlas["questResults/bigRedSkull"])
                {
                    X = posX + i * 4f + 20f * (i + 0.5f),
                    Y = 30f,
                    Color = stats[i].CompletedHardcore ? Color.White : Color.Black
                };

                image.CenterOrigin();
                graphics.Add(image);
            }
            for (int i = 0; i < darkWorldCount; i++)
            {
                Image image = new Image(TFGame.MenuAtlas["questResults/bigGoldSkull"])
                {
                    X = posX + i * 4f + 20f * (i + 0.5f),
                    Y = 55f,
                    Color = stats[i].CompletedLegendary ? Color.White : Color.Black
                };

                image.CenterOrigin();
                graphics.Add(image);
            }
            for (int i = 0; i < darkWorldCount; i++)
            {
                Image image = new Image(TFGame.MenuAtlas["questResults/bigEye"])
                {
                    X = posX + i * 4f + 20f * (i + 0.5f),
                    Y = 80f,
                    Color = stats[i].EarnedEye ? Color.White : Color.Black
                };

                image.CenterOrigin();
                graphics.Add(image);
            }
            for (int i = 0; i < darkWorldCount; i++)
            {
                Image image = new Image(TFGame.MenuAtlas["questResults/bigGoldEye"])
                {
                    X = posX + i * 4f + 20f * (i + 0.5f),
                    Y = 105f,
                    Color = stats[i].EarnedGoldEye ? Color.White : Color.Black
                };

                image.CenterOrigin();
                graphics.Add(image);
            }
        }
        else
        {
            int totalWhiteSkulls = stats.Count(x => x.CompletedNormal);
            int totalRedSkulls = stats.Count(x => x.CompletedHardcore);
            int totalGoldSkulls = stats.Count(x => x.CompletedLegendary);

            int totalEye = stats.Count(x => x.EarnedEye);
            int totalGoldEye = stats.Count(x => x.EarnedGoldEye);

            Image skullImage = new Image(TFGame.MenuAtlas["questResults/bigWhiteSkull"]);
            skullImage.CenterOrigin();
            graphics.Add(skullImage);

            var whiteSkullsText = new OutlineText(TFGame.Font, totalWhiteSkulls + " / " + darkWorldCount.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };
            graphics.Add(whiteSkullsText);

            float posX = skullImage.Width + 10f + whiteSkullsText.Width * whiteSkullsText.Scale.X;
            skullImage.Position = new Vector2(-posX / 2f + skullImage.Width / 2f, 0f);
            whiteSkullsText.Position = new Vector2(posX / 2f - whiteSkullsText.Width / 2f * whiteSkullsText.Scale.X, 0f);

            skullImage = new Image(TFGame.MenuAtlas["questResults/bigRedSkull"]);
            skullImage.CenterOrigin();
            graphics.Add(skullImage);

            var redSkullsText = new OutlineText(TFGame.Font, totalRedSkulls + " / " + darkWorldCount.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };
            graphics.Add(redSkullsText);

            posX = skullImage.Width + 10f + redSkullsText.Width * redSkullsText.Scale.X;
            skullImage.Position = new Vector2(-posX / 2f + skullImage.Width / 2f, 25f);
            redSkullsText.Position = new Vector2(posX / 2f - redSkullsText.Width / 2f * redSkullsText.Scale.X, 25f);

            skullImage = new Image(TFGame.MenuAtlas["questResults/bigGoldSkull"]);
            skullImage.CenterOrigin();
            graphics.Add(skullImage);

            var goldSkullText = new OutlineText(TFGame.Font, totalGoldSkulls + " / " + darkWorldCount.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };

            graphics.Add(goldSkullText);

            posX = skullImage.Width + 10f + goldSkullText.Width * goldSkullText.Scale.X;
            skullImage.Position = new Vector2(-posX / 2f + skullImage.Width / 2f, 50f);
            goldSkullText.Position = new Vector2(posX / 2f - goldSkullText.Width / 2f * goldSkullText.Scale.X, 50f);

            skullImage = new Image(TFGame.MenuAtlas["questResults/bigEye"]);
            skullImage.CenterOrigin();

            graphics.Add(skullImage);

            var eyeText = new OutlineText(TFGame.Font, totalEye + " / " + darkWorldCount.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };

            graphics.Add(eyeText);

            posX = skullImage.Width + 10f + eyeText.Width * eyeText.Scale.X;
            skullImage.Position = new Vector2(-posX / 2f + skullImage.Width / 2f, 75f);
            eyeText.Position = new Vector2(posX / 2f - eyeText.Width / 2f * eyeText.Scale.X, 75f);

            skullImage = new Image(TFGame.MenuAtlas["questResults/bigGoldEye"]);
            skullImage.CenterOrigin();

            graphics.Add(skullImage);

            var goldEyeText = new OutlineText(TFGame.Font, totalGoldEye + " / " + darkWorldCount.ToString(), Vector2.Zero, Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
            {
                Scale = Vector2.One * 2f
            };

            graphics.Add(goldEyeText);

            posX = skullImage.Width + 10f + goldEyeText.Width * goldEyeText.Scale.X;
            skullImage.Position = new Vector2(-posX / 2f + skullImage.Width / 2f, 100f);
            goldEyeText.Position = new Vector2(posX / 2f - goldEyeText.Width / 2f * goldEyeText.Scale.X, 100f);
        }


        graphics.Add(new OutlineText(TFGame.Font, "TOTAL ATTEMPTS:", new Vector2(-60f, 130f), SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center));
        graphics.Add(new OutlineText(TFGame.Font, SaveData.Instance.DarkWorld.TotalAttempts.ToString(), new Vector2(-60f, 142f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });

        graphics.Add(new OutlineText(TFGame.Font, "TOTAL DEATHS:", new Vector2(60f, 130f), SubtitleColor, Text.HorizontalAlign.Center, Text.VerticalAlign.Center));
        graphics.Add(new OutlineText(TFGame.Font, SaveData.Instance.DarkWorld.TotalDeaths.ToString(), new Vector2(60f, 142f), Text.HorizontalAlign.Center, Text.VerticalAlign.Center)
        {
            Scale = Vector2.One * 2f
        });

        this.Add(graphics);
    }
}
