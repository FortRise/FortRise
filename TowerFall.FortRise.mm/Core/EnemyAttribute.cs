using System;

namespace FortRise;

/// <summary>
/// An attribute marker that loads an enemy when matching names is detected.
/// <br/>
/// It supports in any level format as long as the entity names is matched.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CustomEnemyAttribute : Attribute 
{
    /// <summary>
    /// A list of names identifiers for this enemy.
    /// </summary>
    public string[] Names;

    /// <summary>
    /// Marked a class derived from enemy as a custom enemy.
    /// </summary>
    /// <param name="names">A list of names identifiers for this enemy</param>
    public CustomEnemyAttribute(params string[] names) 
    {
        Names = names;
    }
}