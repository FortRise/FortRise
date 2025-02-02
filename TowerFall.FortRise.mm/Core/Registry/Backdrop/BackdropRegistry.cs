using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using TowerFall;

namespace FortRise;

public static class BackdropRegistry 
{
    public delegate Background.BGElement BGElementLoader(Level level, XmlElement xmlElement);

    public static Dictionary<string, BGElementLoader> BGElements = new Dictionary<string, BGElementLoader>();

    public static void Register(Type type, FortModule module) 
    {
        foreach (var backdrop in type.GetCustomAttributes<CustomBackdropAttribute>()) 
        {
            if (type is null)
            {
                return;
            }

            var name = backdrop.Name;
            ConstructorInfo ctor = type.GetConstructor(new Type[2] { typeof(Level), typeof(XmlElement) });

            if (ctor != null)
            {
                BGElementLoader loader = (level, element) => {
                    return (Background.BGElement)ctor.Invoke(new object[2] { level, element });
                };

                BGElements[name] = loader;
            }
            else 
            {
                Logger.Error($"[BackdropLoader] [{type.Name}] Constructor (TowerFall.Level, System.Xml.XmlElement) couldn't be found!");
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomBackdropAttribute : Attribute
{
    public string Name { get; set; }
    public CustomBackdropAttribute(string name) 
    {
        Name = name;
    }
}