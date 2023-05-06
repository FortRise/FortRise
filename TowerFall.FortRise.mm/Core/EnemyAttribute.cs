using System;

namespace FortRise;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class EnemyAttribute : Attribute 
{
    public string Name;
    public string FuncArg;


    public EnemyAttribute(string name, string arg = null) 
    {
        Name = name;
        FuncArg = arg;
    }
}