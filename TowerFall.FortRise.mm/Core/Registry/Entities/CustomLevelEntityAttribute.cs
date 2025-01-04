using System;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomLevelEntityAttribute : Attribute 
{
    public string[] Names;

    public CustomLevelEntityAttribute(string name) 
    {
        Names = new string[1] { name };
    }

    public CustomLevelEntityAttribute(params string[] names) 
    {
        Names = names;
    }
}