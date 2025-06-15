using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace FortRise.Levels;

internal sealed class LevelsModule : Mod
{
    public static LevelsModule Instance = null!;


    public LevelsModule(IModContent content, IModuleContext context) : base(content, context)
    {
        Instance = this;
        context.Events.OnModLoadingFinished += OnModLoadingFinished;
    }

    private void OnModLoadingFinished(object? sender, EventArgs e)
    {
        var dependents = Context.Interop.GetModDependents();
        for (int i = 0; i < dependents.Count; i++)
        {
            var dependent = dependents[i];
            var registry = Context.Interop.GetModRegistry(dependent.Metadata);
            if (registry is null)
            {
                continue;
            }

            TilesetLoader.Load(registry, dependent.Content);
            ThemeLoader.Load(registry, dependent.Content);
            VersusLoader.Load(registry, dependent.Content);
            QuestLoader.Load(registry, dependent.Content);
            DarkWorldLoader.Load(registry, dependent.Content);
            TrialsLoader.Load(registry, dependent.Content);
        }
    }
}

internal static class TilesetLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/GameData/tilesetData.xml", out IResourceInfo theme))
        {
            return;
        }

        var xml = theme.Xml?["TilesetData"];
        if (xml is null)
        {
            return;
        }

        foreach (XmlElement xmlTileset in xml)
        {
            var themeID = xmlTileset.Attr("id");

            var image = xmlTileset.Attr("image");
            ISubtextureEntry texture = null!;

            if (content.Root.TryGetRelativePath(image, out var info))
            {
                texture = content.LoadTexture(info);
            }
            else
            {
                texture = content.LoadTexture(() => TFGame.Atlas[image]);
            }

            registry.Tilesets.RegisterTileset(themeID, new()
            {
                Texture = texture,
                AutotileData = new AutotileData(xmlTileset)
            });
        }
    }
}

internal static class ThemeLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/GameData/themeData.xml", out IResourceInfo theme))
        {
            return;
        }

        var xml = theme.Xml?["ThemeData"];
        if (xml is null)
        {
            return;
        }

        foreach (XmlElement xmlTheme in xml)
        {
            var icon = xmlTheme.ChildText("Icon");
            ISubtextureEntry subIcon = null!;

            if (content.Root.TryGetRelativePath(icon, out var info))
            {
                subIcon = content.LoadTexture(info);
            }
            else
            {
                subIcon = content.LoadTexture(() => TFGame.MenuAtlas["towerIcons/" + icon]);
            }

            var themeID = xmlTheme.Attr("id", xmlTheme.Name);
            registry.Themes.RegisterTheme(themeID, new()
            {
                Name = xmlTheme.ChildText("Name").Trim().ToUpperInvariant(),
                Icon = subIcon,
                TowerType = xmlTheme.ChildEnum<MapButton.TowerType>("TowerType", MapButton.TowerType.Normal),
                MapPosition = xmlTheme["MapPosition"].Position(),
                Music = xmlTheme.ChildText("Music", string.Empty),
                DarknessColor = xmlTheme.ChildHexColor("DarknessColor", Color.Black),
                DarknessOpacity = xmlTheme.ChildFloat("DarknessOpacity", 0.2f),
                Wind = xmlTheme.ChildInt("Wind", 0),
                Lanterns = xmlTheme.ChildEnum<TowerTheme.LanternTypes>("Lanterns", TowerTheme.LanternTypes.CathedralTorch),
                World = xmlTheme.ChildEnum<TowerTheme.Worlds>("World", TowerTheme.Worlds.Normal),
                Raining = xmlTheme.ChildBool("Raining", false),
                BackgroundID = xmlTheme.ChildText("Background"),
                DrillParticleColor = xmlTheme.ChildHexColor("DrillParticleColor", Color.Red),
                Cold = xmlTheme.ChildBool("Cold", false),
                CrackedBlockColor = xmlTheme.ChildHexColor("CrackedBlockColor", "4EB1E9"),
                Tileset = xmlTheme.ChildText("Tileset", "SacredGround"),
                BGTileset = xmlTheme.ChildText("BGTileset", "SacredGroundBG"),
                Cataclysm = xmlTheme.ChildBool("Cataclysm", xmlTheme.ChildText("Tileset") == "Cataclysm")
            });
        }
    }

    public static string LoadInlineTheme(XmlElement xml, IModContent content, IModRegistry registry)
    {
        if (xml.HasChild("theme"))
        {
            var xmlTheme = xml["theme"];
            if (xmlTheme.HasChild("Name"))
            {
                // load inline themes
                var icon = xmlTheme.ChildText("Icon");
                ISubtextureEntry subIcon = null!;

                if (content.Root.TryGetRelativePath(icon, out var info))
                {
                    subIcon = content.LoadTexture(info);
                }
                else
                {
                    subIcon = content.LoadTexture(() => TFGame.MenuAtlas["towerIcons/" + icon]);
                }

                var themeLoaded = registry.Themes.RegisterTheme(Guid.CreateVersion7().ToString(), new()
                {
                    Name = xmlTheme.ChildText("Name").Trim().ToUpperInvariant(),
                    Icon = subIcon,
                    TowerType = xmlTheme.ChildEnum<MapButton.TowerType>("TowerType", MapButton.TowerType.Normal),
                    MapPosition = xmlTheme!["MapPosition"].Position(),
                    Music = xmlTheme.ChildText("Music", string.Empty),
                    DarknessColor = xmlTheme.ChildHexColor("DarknessColor", Color.Black),
                    DarknessOpacity = xmlTheme.ChildFloat("DarknessOpacity", 0.2f),
                    Wind = xmlTheme.ChildInt("Wind", 0),
                    Lanterns = xmlTheme.ChildEnum<TowerTheme.LanternTypes>("Lanterns", TowerTheme.LanternTypes.CathedralTorch),
                    World = xmlTheme.ChildEnum<TowerTheme.Worlds>("World", TowerTheme.Worlds.Normal),
                    Raining = xmlTheme.ChildBool("Raining", false),
                    BackgroundID = xmlTheme.ChildText("Background"),
                    DrillParticleColor = xmlTheme.ChildHexColor("DrillParticleColor", Color.Red),
                    Cold = xmlTheme.ChildBool("Cold", false),
                    CrackedBlockColor = xmlTheme.ChildHexColor("CrackedBlockColor", "4EB1E9"),
                    Tileset = xmlTheme.ChildText("Tileset", "SacredGround"),
                    BGTileset = xmlTheme.ChildText("BGTileset", "SacredGroundBG"),
                    Cataclysm = xmlTheme.ChildBool("Cataclysm", xmlTheme.ChildText("Tileset") == "Cataclysm")
                });

                return themeLoaded.Name;
            }
            else
            {
                return xml.ChildText("theme").Trim();
            }
        }
        return "SacredGround";
    }
}

internal static class DarkWorldLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Levels/DarkWorld", out IResourceInfo darkWorldLocation))
        {
            return;
        }

        foreach (var map in darkWorldLocation.Childrens)
        {
            var path = map.FullPath.Substring(4).Replace("Content/Levels/DarkWorld/", string.Empty);

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

internal static class TrialsLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Levels/Trials", out IResourceInfo trialLocation))
        {
            return;
        }

        foreach (var map in trialLocation.Childrens)
        {
            IResourceInfo? towerResource = null;
            foreach (var child in map.Childrens)
            {
                if (child.Path.Contains("tower.xml"))
                {
                    towerResource = child;
                    break;
                }
            }
            if (towerResource == null)
            {
                continue;
            }

            using var xmlStream = towerResource.Stream;
            var xml = Calc.LoadXML(xmlStream)["tower"];

            if (xml.HasChild("tier"))
            {
                xml = xml!["tier"];
            }

            var arr = new TrialsLevelData[3];
            TrialsTier[] tiers = new TrialsTier[3];
            int i = 0;
            foreach (XmlElement element in xml!.GetElementsByTagName("level"))
            {
                if (i == 3)
                {
                    Logger.Warning($"[{content.Metadata.Name}] The trials are only limited to 3 tiers only.");
                    break;
                }
                var tier = new TrialsTier()
                {
                    Arrows = element.ChildInt("arrows", 3),
                    SwitchBlockTimer = element.ChildInt("switchTimer", 200),
                    Level = map.GetRelativePath(element.Attr("path")),
                    Theme = ThemeLoader.LoadInlineTheme(element, content, registry),
                    DevTime = element.ChildFloat("dev", 0.3f),
                    DiamondTime = element.ChildFloat("diamond", 0.2f),
                    GoldTime = element.ChildFloat("gold", 0.1f),
                };


                tiers[i] = tier;
                i += 1;
            }

            if (i != 3)
            {
                Logger.Error($"[{content.Metadata.Name}] Not enough Trial levels are able to load this tower. Must have exactly 3 levels.");
                return;
            }

            registry.Towers.RegisterTrialTower(Path.GetFileName(map.Path), new()
            {
                Author = xml.ChildText("author", string.Empty),
                Tier1 = tiers[0],
                Tier2 = tiers[1],
                Tier3 = tiers[2]
            });
        }
    }
}

internal static class QuestLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Levels/Quest", out IResourceInfo questLocation))
        {
            return;
        }

        foreach (var map in questLocation.Childrens)
        {
            var fullPath = map.FullPath;

            IResourceInfo? towerXmlResource = null;
            foreach (var child in map.Childrens)
            {
                if (!child.Path.Contains("tower.xml"))
                {
                    continue;
                }

                towerXmlResource = child;
                break;
            }

            if (towerXmlResource is null)
            {
                continue;
            }

            using var xmlStream = towerXmlResource.Stream;
            var xml = Calc.LoadXML(xmlStream)["tower"];

            registry.Towers.RegisterQuestTower(Path.GetFileName(map.Path), new()
            {
                Author = xml.ChildText("author", string.Empty),
                Theme = ThemeLoader.LoadInlineTheme(xml!, content, registry),
                Level = map.GetRelativePath("level.oel"),
                Data = map.GetRelativePath("data.xml")
            });
        }
    }
}

internal static class VersusLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Levels/Versus", out IResourceInfo versusLocation))
        {
            return;
        }

        foreach (var map in versusLocation.Childrens)
        {
            List<IResourceInfo> levels = new List<IResourceInfo>();

            IResourceInfo? xmlResource = null;
            foreach (var child in map.Childrens)
            {
                if ((child.ResourceType == typeof(RiseCore.ResourceTypeOel) ||
                child.ResourceType == typeof(RiseCore.ResourceTypeJson)) &&
                !child.Path.StartsWith("icon"))
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
            List<Treasure> treasures = new List<Treasure>();
            if (xml.HasChild("treasure"))
            {
                var array = xml.ChildText("treasure").Split(',');
                for (int i = 0; i < array.Length; i++)
                {
                    var treasure = array[i];
                    ParseTreasure(treasure.AsSpan().Trim(), out string resTreasure, out int chance, out int rate);
                    if (!Calc.TryStringToEnum<Pickups>(resTreasure, out var pickups))
                    {
                        Logger.Error($"[ADVENTURE][VERSUS] The pickup name '{resTreasure}' cannot be found.");
                        continue;
                    }
                    treasures.Add(new Treasure()
                    {
                        Pickup = pickups,
                        Rates = rate,
                        Chance = (chance / 100.0f)
                    });
                }
            }

            var filename = Path.GetFileName(map.Path);

            registry.Towers.RegisterVersusTower(filename, new()
            {
                Levels = levels.ToArray(),
                Theme = ThemeLoader.LoadInlineTheme(xml!, content, registry),
                ArrowShuffle = xml!["treasure"].AttrBool("arrowShuffle", false),
                SpecialArrowRate = xml!["treasure"].AttrFloat("arrows", 0.6f),
                Author = xml.ChildText("author", string.Empty),
                Treasure = treasures.ToArray()
            });
        }
    }

    private static void ParseTreasure(ReadOnlySpan<char> treasure, out string resultTreasure, out int chance, out int rate)
    {
        if (treasure.IndexOf('[') != 0)
        {
            chance = -1;
            rate = -1;
            resultTreasure = treasure.ToString();
            return;
        }

        var text = treasure.Slice(1, treasure.IndexOf(']') - 1);
        resultTreasure = treasure.Slice(treasure.IndexOf(']') + 1).ToString();
        var split = text.SplitLines('*');
        chance = -1;
        rate = -1;
        foreach (var sp in split)
        {
            var numText = sp.Line;
            if (numText.Contains("%".AsSpan(), StringComparison.InvariantCulture))
            {
                var chanceSlice = numText.Slice(0, numText.IndexOf('%'));
                chance = int.Parse(chanceSlice.ToString());
                continue;
            }
            rate = int.Parse(numText.ToString());
        }
    }
}