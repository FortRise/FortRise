using System.IO;
using Monocle;

namespace FortRise.Levels;

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
