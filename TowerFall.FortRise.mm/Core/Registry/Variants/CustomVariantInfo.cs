using Monocle;
using TowerFall;

namespace FortRise;

/// <summary>
/// A struct that represents a information for variant.
/// </summary>
public struct CustomVariantInfo 
{
    /// <summary>
    /// Represents an name and id for the variant.
    /// </summary>
    public string Name;
    /// <summary>
    /// Adds a description to the given variant
    /// </summary>
    public string Description;
    /// <summary>
    /// A header to organize the variant in some way.
    /// </summary>
    public string Header;
    /// <summary>
    /// A flags on how the variants will behave in the list.
    /// </summary>
    public CustomVariantFlags Flags;
    /// <summary>
    /// An icon for the variant to show in the list. 
    /// </summary>
    public Subtexture Icon;
    /// <summary>
    /// What pickup to not appear when this variant is enabled.
    /// </summary>
    public Pickups[] Exclusions;

    public CustomVariantInfo(string name, CustomVariantFlags flag = CustomVariantFlags.None) 
    {
        Name = name;
        Icon = null;
        Flags = flag;
        Description = string.Empty;
        Exclusions = null;
        Header = null;
    }

    public CustomVariantInfo(string name, Subtexture texture, CustomVariantFlags flag = CustomVariantFlags.None) 
    {
        Name = name;
        Icon = texture;
        Flags = flag;
        Description = string.Empty;
        Exclusions = null;
        Header = null;
    }

    public CustomVariantInfo(string name, Subtexture icon, CustomVariantFlags flags = CustomVariantFlags.None, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = icon;
        Flags = flags;
        Description = string.Empty;
        Exclusions = exclusions;
        Header = null;
    }

    public CustomVariantInfo(string name, CustomVariantFlags flags = CustomVariantFlags.None, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = null;
        Flags = flags;
        Description = string.Empty;
        Exclusions = exclusions;
        Header = null;
    }

    public CustomVariantInfo(string name, Subtexture icon, string description, CustomVariantFlags flags = CustomVariantFlags.None) 
    {
        Name = name;
        Icon = icon;
        Flags = flags;
        Description = description;
        Exclusions = null;
        Header = null;
    }

    public CustomVariantInfo(string name, string description, CustomVariantFlags flags = CustomVariantFlags.None) 
    {
        Name = name;
        Icon = null;
        Flags = flags;
        Description = description;
        Exclusions = null;
        Header = null;
    }

    public CustomVariantInfo(string name, Subtexture icon, string description, CustomVariantFlags flags, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = icon;
        Flags = flags;
        Description = description;
        Exclusions = exclusions;
        Header = null;
    }

    public CustomVariantInfo(string name, string description, CustomVariantFlags flags, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = null;
        Flags = flags;
        Description = description;
        Exclusions = exclusions;
        Header = null;
    }

    public CustomVariantInfo(string name, Subtexture icon, string description, string header, CustomVariantFlags flags, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = icon;
        Flags = flags;
        Description = description;
        Exclusions = exclusions;
        Header = header;
    }

    public CustomVariantInfo(string name, string description, string header, CustomVariantFlags flags, params Pickups[] exclusions) 
    {
        Name = name;
        Icon = null;
        Flags = flags;
        Description = description;
        Exclusions = exclusions;
        Header = header;
    }
}
