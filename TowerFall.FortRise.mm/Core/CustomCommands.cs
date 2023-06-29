using System;

namespace FortRise;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute 
{
    public string CommandName;

    public CommandAttribute(string commandName) 
    {
        CommandName = commandName;
    }
}