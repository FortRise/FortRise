using System;

namespace FortRise;

public abstract class ModuleSettings {}

[AttributeUsage(AttributeTargets.Field)]
public class SettingsNameAttribute : Attribute 
{
    public string Name;

    public SettingsNameAttribute(string name) 
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingsNumberAttribute : Attribute 
{
    public int Min;
    public int Max;
    public int Step;

    public SettingsNumberAttribute(int min = 0, int max = 100, int step = 1) 
    {
        Min = min;
        Max = max;
        Step = step;
    }
}