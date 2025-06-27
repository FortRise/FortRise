using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Monocle;
using TowerFall;

namespace FortRise.Levels;

internal static class VersusLoader
{
    internal static void Load(IModRegistry registry, IModContent content, ILogger logger)
    {
        if (!content.Root.TryGetRelativePath("Content/Levels/Versus", out IResourceInfo versusLocation))
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
                        logger.LogError("The pickup name '{resTreasure}' cannot be found.", resTreasure);
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