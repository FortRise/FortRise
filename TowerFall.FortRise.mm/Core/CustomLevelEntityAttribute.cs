using System;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomLevelEntityAttribute : Attribute 
{
    public string Name;

    public CustomLevelEntityAttribute(string name) 
    {
        Name = name;
    }
}