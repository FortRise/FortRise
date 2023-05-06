using System;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CustomEnemyAttribute : Attribute 
{
    public string[] Names;

    public CustomEnemyAttribute(params string[] names) 
    {
        Names = names;
    }
}