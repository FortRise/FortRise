using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using TowerFall;

namespace FortRise;

public static class BackdropRegistry 
{
    public delegate Background.BGElement BGElementLoader(Level level, XmlElement xmlElement);

    public static Dictionary<string, BGElementLoader> BGElements = new Dictionary<string, BGElementLoader>();

    public static void Register(string name, BackdropConfiguration configuration)
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
