using System;

namespace FortRise;

/// <summary>
/// An attribute marker that adds a command to the developer console from a function.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute 
{
    /// <summary>
    /// A name of the command.
    /// </summary>
    public string CommandName;

    /// <summary>
    /// Marked a function to add a command to the developer console.
    /// </summary>
    /// <param name="commandName">A name of the command</param>
    public CommandAttribute(string commandName) 
    {
        CommandName = commandName;
    }
}