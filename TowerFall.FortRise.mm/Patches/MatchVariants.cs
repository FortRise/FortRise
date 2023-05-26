using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MatchVariants : MatchVariants 
{
    private Dictionary<string, Variant> customVariants;
    public IReadOnlyDictionary<string, Variant> CustomVariants => customVariants;

    internal static event Action<MatchVariants, bool> DeclareVariants;

    /* Private fields from MatchVariants */
    private List<Variant> canRandoms;
    [MonoModIgnore]
    public static int Count { get; private set; }

    public void orig_ctor(bool noPerPlayer = false) {}

    [MonoModConstructor]
    public void ctor(bool noPerPlayer = false) 
    {
        orig_ctor(noPerPlayer);
        customVariants = new();
        DeclareVariants?.Invoke(this, noPerPlayer);
    }


    public Variant AddVariant(string variantName, VariantInfo info, VariantFlags flags, bool noPerPlayer) 
    {
        TempVariantHolder.TempCustom ??= new Dictionary<string, bool>();
        var list = Variants.ToList();
        Pickups[] itemExclusions = info.Exclusions;
        bool perPlayer = flags.HasFlag(VariantFlags.PerPlayer) && !noPerPlayer;
        string description = info.Description;
        string header = info.Header;
        bool scrollEffect = flags.HasFlag(VariantFlags.ScrollEffect);
        bool hidden = flags.HasFlag(VariantFlags.Hidden);
        bool flag = flags.HasFlag(VariantFlags.CanRandom);
        bool tournamentRule1v = flags.HasFlag(VariantFlags.TournamentRule1v1);
        bool tournamentRule2v = flags.HasFlag(VariantFlags.TournamentRule2v2);
        bool unlisted = flags.HasFlag(VariantFlags.Unlisted);
        bool darkWorldDLC = flags.HasFlag(VariantFlags.DarkWorldDLC);
        int coopValue = 0;
        if (flags.HasFlag(VariantFlags.CoopCurses)) 
        {
            coopValue = -1;
        }
        else if (flags.HasFlag(VariantFlags.CoopBlessing))
        {
            coopValue = 1;
        }
        var title = GetCustomVariantTitle(variantName);
        var variant = new Variant(
            GetVariantIconFromName(variantName, info.VariantAtlas), title, description, 
            itemExclusions, perPlayer, header, null, scrollEffect, hidden, flag, tournamentRule1v, 
            tournamentRule2v, unlisted, darkWorldDLC, coopValue);
        customVariants.Add(variantName, variant);
        list.Add(variant);
        if (flag)
            canRandoms.Add(variant);
        Variants = list.ToArray();
        Count = list.Count;
        if (!TempVariantHolder.TempCustom.ContainsKey(variantName))
            TempVariantHolder.TempCustom.Add(variantName, false);
        return variant;
    }
    public static Subtexture GetVariantIconFromName(string variantName, Atlas atlas)
    {
        return atlas["variants/" + variantName[0].ToString().ToLower(CultureInfo.InvariantCulture) + variantName.Substring(1)];
    }

    [Obsolete("Use `Calc.IncompatibleWith` extension method")]
    public void CreateCustomLinks(params Variant[] variants)
    {
        for (int i = 0; i < variants.Length; i++)
        {
            variants[i].AddLinks(variants);
        }
    }

    public Variant GetCustomVariant(string name) 
    {
        return customVariants[name];
    }

    private static string GetCustomVariantTitle(string name)
    {
        string text = name;
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                text = text.Substring(0, i) + " " + text.Substring(i);
                i++;
            }
        }
        return text.ToUpper(CultureInfo.InvariantCulture);
    }
}

public struct VariantInfo 
{
    public Atlas VariantAtlas;
    public string Header = "";
    public string Description = "";
    public Version NewInVersion;
    public Pickups[] Exclusions;

    public static readonly VariantInfo Empty = new VariantInfo();

    public VariantInfo(Atlas variantAtlas)
    {
        VariantAtlas = variantAtlas;
        Header = "";
        Description = "";
        Exclusions = null;
        NewInVersion = null;
    }
}

[Flags]
public enum VariantFlags
{
    None,
    PerPlayer,
    CanRandom,
    ScrollEffect,
    CoopBlessing,
    CoopCurses,
    TournamentRule1v1,
    TournamentRule2v2,
    DarkWorldDLC,
    Hidden,
    Unlisted,
}