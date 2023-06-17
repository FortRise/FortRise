using System;

namespace TowerFall;

public sealed class CustomRoundLogicAttribute : Attribute 
{
    public string Name;
    public string OverrideFunction;

    public CustomRoundLogicAttribute(string name, string overriden = null) 
    {
        Name = name;
        OverrideFunction = overriden;
    }
}