using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.Content;

internal static class DarkWorldLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Content/Levels/DarkWorld", out IResourceInfo darkWorldLocation))
        {
            return;
        }


        foreach (var map in darkWorldLocation.Childrens)
        {
            var levels = new List<IResourceInfo>();
            IResourceInfo? xmlResource = null;
            foreach (var child in map.Childrens)
            {
                if ((child.ResourceType == typeof(RiseCore.ResourceTypeOel) ||
                child.ResourceType == typeof(RiseCore.ResourceTypeJson)) &&
                !child.Path.Contains("icon.json"))
                {
                    levels.Add(child);
                    continue;
                }

                if (child.Path.Contains("tower.xml"))
                {
                    xmlResource = child;
                }
            }

            if (xmlResource == null)
            {
                continue;
            }

            using var xmlStream = xmlResource.Stream;
            var xml = Calc.LoadXML(xmlStream)["tower"];


            var enemySets = new Dictionary<string, List<DarkWorldTowerData.EnemyData>>();
            foreach (XmlElement xmlElement2 in xml!["enemies"]!.GetElementsByTagName("set"))
            {
                string text3 = xmlElement2.Attr("id");
                var list = new List<DarkWorldTowerData.EnemyData>();
                foreach (XmlElement xmlElement3 in xmlElement2.GetElementsByTagName("spawn"))
                {
                    list.Add(new DarkWorldTowerData.EnemyData(xmlElement3));
                }
                enemySets.Add(text3, list);
            }

            var normalLevels = GetLevelData(xml["normal"]!);
            var hardcoreLevels = GetLevelData(xml["hardcore"]!);
            var legendaryLevels = GetLevelData(xml["legendary"]!);

            registry.Towers.RegisterDarkWorldTower(Path.GetFileName(map.Path), new()
            {
                Levels = levels.ToArray(),
                Normal = normalLevels.ToArray(),
                Hardcore = hardcoreLevels.ToArray(),
                Legendary = legendaryLevels.ToArray(),
                EnemySets = enemySets,
                Theme = ThemeLoader.LoadInlineTheme(xml, content, registry),
                Author = xml.ChildText("author", string.Empty),
                StartingLives = xml.ChildInt("lives", -1),
                TimeBase = xml["time"]?.ChildInt("base", 300) ?? 300,
                TimeAdd = xml["time"]?.ChildInt("add", 40) ?? 40,
                MaxContinues = [
                    xml["continues"]?.ChildInt("normal") ?? -1,
                    xml["continues"]?.ChildInt("hardcore") ?? -1,
                    xml["continues"]?.ChildInt("legendary") ?? -1
                ]
            });

            static List<DarkWorldLevelData> GetLevelData(XmlElement element)
            {
                var levels = new List<DarkWorldLevelData>();
                foreach (XmlElement normalXml in element.GetElementsByTagName("level"))
                {
                    Option<int> boss;
                    if (normalXml.HasChild("boss"))
                    {
                        var text = normalXml.InnerText.Trim();
                        if (int.TryParse(text, out int bossID))
                        {
                            boss = bossID;
                        }
                        else if (DarkWorldBossRegistry.DarkWorldBosses.TryGetValue(text, out int bossID2))
                        {
                            boss = bossID2;
                        }
                        else
                        {
                            boss = 0;
                        }
                    }
                    else
                    {
                        boss = Option<int>.None();
                    }

                    ConstraintedTreasure[]? treasure = null;

                    if (normalXml.HasChild("treasure"))
                    {
                        var protoTreasure = new List<ConstraintedTreasure>();
                        foreach (string text in Calc.ReadCSV(normalXml.ChildText("treasure")))
                        {
                            string newText = text;
                            int min = 0;
                            int max = 0;
                            while (newText[0] == '+')
                            {
                                newText = newText.Substring(1);
                                min += 1;
                            }

                            while (newText[0] == '-')
                            {
                                newText = newText.Substring(1);
                                max += 1;
                            }

                            if (max == 0)
                            {
                                max = 4;
                            }

                            Pickups pickups = Calc.StringToEnum<Pickups>(newText);
                            protoTreasure.Add(new ConstraintedTreasure()
                            {
                                Pickups = pickups,
                                MinPlayer = min,
                                MaxPlayer = max
                            });
                        }

                        treasure = protoTreasure.ToArray();
                    }

                    string[] variants = [];

                    if (normalXml.HasChild("variants"))
                    {
                        if (normalXml.FirstChild!.NodeType == XmlNodeType.Text)
                        {
                            variants = Calc.ReadCSV(normalXml["variants"]!.InnerText);
                        }
                        else
                        {
                            List<string> rawVariants = [];
                            foreach (XmlElement variant in normalXml["variants"]!)
                            {
                                rawVariants.Add(variant.Name.Trim());
                            }
                            variants = rawVariants.ToArray();
                        }
                    }

                    levels.Add(new()
                    {
                        BossID = boss,
                        Treasures = treasure,
                        LevelIndex = normalXml.ChildInt("file", 0),
                        Difficulty = normalXml.ChildInt("difficulty", 0),
                        Waves = normalXml.ChildInt("waves", 3),
                        EnemySet = normalXml.ChildText("enemySet", "Normal"),
                        DelayMultiplier = normalXml.ChildFloat("delayMultiplier", 1),
                        Variants = variants
                    });
                }

                return levels;
            }
        }
    }
}
