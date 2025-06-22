using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;
using Monocle;
using TowerFall;

namespace FortRise.Levels;

internal static class TrialsLoader
{
    internal static void Load(IModRegistry registry, IModContent content, ILogger logger)
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
                    logger.LogWarning("The trials are only limited to 3 tiers only.");
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
                logger.LogError("Not enough Trial levels are able to load this tower. Must have exactly 3 levels.");
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
