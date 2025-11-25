using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using TowerFall;

namespace FortRise;

public static class BGElementsRegistry 
{
    public delegate Background.BGElement BGElementLoader(Level level, XmlElement xmlElement);

    private static Dictionary<string, IBGElementEntry> backdropEntries = [];
    public static Dictionary<string, BGElementLoader> BGElements = new Dictionary<string, BGElementLoader>();

    public static void AddBGElement(IBGElementEntry backdropEntry)
    {
        backdropEntries[backdropEntry.Name] = backdropEntry;
    }

#nullable enable
    public static IBGElementEntry? GetBGElement(string id)
    {
        backdropEntries.TryGetValue(id, out var entry);
        return entry;
    }
#nullable disable

    public static void Register(string name, BGElementConfiguration configuration)
    {
        ConstructorInfo ctor = configuration.BackdropType.GetConstructor([typeof(Level), typeof(XmlElement)]);

        if (ctor != null)
        {
            Background.BGElement loader(Level level, XmlElement element)
            {
                return (Background.BGElement)ctor.Invoke([level, element]);
            }

            BGElements[name] = loader;
        }
        else
        {
            Logger.Error($"[BackdropLoader] [{name}] Constructor (TowerFall.Level, System.Xml.XmlElement) couldn't be found!");
        }
    }
}
