#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public class patch_MatchVariants : MatchVariants 
{
    internal Dictionary<string, Variant> InternalCustomVariants;
    public IReadOnlyDictionary<string, Variant> CustomVariants => InternalCustomVariants;

    public List<(Variant, ArrowTypes)> StartWithVariants;

    private List<Variant> canRandoms;
    [MonoModIgnore]
    public static int Count { get; private set; }
    internal VariantManager manager;

    public void orig_ctor(bool noPerPlayer = false) {}

    [MonoModConstructor]
    public void ctor(bool noPerPlayer = false) 
    {
        InternalCustomVariants = new();
        StartWithVariants = new();

        orig_ctor(noPerPlayer);

        TempVariantHolder.TempCustom = new Dictionary<string, bool>();
        manager = new VariantManager(this);
        foreach (var mod in RiseCore.ModuleManager.InternalFortModules) 
        {
            manager.SetContext(mod.Meta);
            mod.OnVariantsRegister(manager, noPerPlayer);
        }


        int oldLength = Variants.Length;
        Array.Resize(ref Variants, manager.TotalCustomVariantsAdded + Variants.Length);
        int count = 0;
        foreach (var key in manager.ToAdd.Keys) 
        {
            var list = manager.ToAdd[key]; 
            for (int i = oldLength; i < oldLength + list.Count; i++) 
            {
                Variants[i] = list[count];
                count++;
            }
            oldLength += count;
            count = 0;
        }

        foreach (var random in manager.CanRandoms)
            canRandoms.Add(random);
        manager.Dispose();

        oldLength = Variants.Length;
        Array.Resize(ref Variants, oldLength + VariantRegistry.Variants.Count);

        count = oldLength;
        foreach (var (name, value) in VariantRegistry.Variants)
        {
            var configuration = value.Configuration;
            var flags = configuration.Flags;
            Pickups[] itemExclusions = configuration.Exclusions;
            bool perPlayer = flags.HasFlag(CustomVariantFlags.PerPlayer);
            string description = configuration.Description?.ToUpperInvariant();
            string header = configuration.Header?.ToUpperInvariant();

            bool scrollEffect = flags.HasFlag(CustomVariantFlags.ScrollEffect);
            bool hidden = flags.HasFlag(CustomVariantFlags.Hidden);
            bool flag = flags.HasFlag(CustomVariantFlags.CanRandom);
            bool tournamentRule1v = flags.HasFlag(CustomVariantFlags.TournamentRule1v1);
            bool tournamentRule2v = flags.HasFlag(CustomVariantFlags.TournamentRule2v2);
            bool unlisted = flags.HasFlag(CustomVariantFlags.Unlisted);
            bool darkWorldDLC = flags.HasFlag(CustomVariantFlags.DarkWorldDLC);
            int coopValue = 0;
            if (flags.HasFlag(CustomVariantFlags.CoopCurses)) 
            {
                coopValue = -1;
            }
            else if (flags.HasFlag(CustomVariantFlags.CoopBlessing))
            {
                coopValue = 1;
            }
            var title = configuration.Title.ToUpperInvariant();
            var icon = configuration.Icon;
            var variant = new Variant(icon, title, description, itemExclusions, perPlayer, 
                header, null, scrollEffect, hidden, flag, tournamentRule1v, 
                tournamentRule2v, unlisted, darkWorldDLC, coopValue);
            if (flag)
            {
                canRandoms.Add(variant);
            }

            TempVariantHolder.TempCustom.TryAdd(name, false);
            InternalCustomVariants.Add(name, variant);
            Variants[count] = variant;
            count++;
        }

        // links
        foreach (var (name, value) in VariantRegistry.Variants)
        {
            var variant = InternalCustomVariants[name];
            var links = value.Configuration.Links;
            if (links == null)
            {
                continue;
            }
            foreach (var link in links)
            {
                if (InternalCustomVariants.TryGetValue(link.Name, out Variant otherVariant))
                {
                    variant.AddLinks(otherVariant);
                    otherVariant.AddLinks(variant);
                }
                else 
                {
                    var field = typeof(MatchVariants).GetField(link.Name);
                    Variant vanillaVariant = field.GetValue(this) as Variant;
                    variant.AddLinks(vanillaVariant);
                    vanillaVariant.AddLinks(variant);
                }
            }
        }

        Count = Variants.Length;
    }

    public Variant GetCustomVariant(string name) 
    {
        return InternalCustomVariants[name];
    }

    public extern ArrowTypes orig_GetStartArrowType(int playerIndex, ArrowTypes randomType);

    public ArrowTypes GetStartArrowType(int playerIndex, ArrowTypes randomType) 
    {
        foreach (var (arrowVariant, arrowTypes) in StartWithVariants)
        {
            if (!arrowVariant[playerIndex])
                continue;
            return arrowTypes;
        }
        return orig_GetStartArrowType(playerIndex, randomType);
    }

    public extern List<Pickups> orig_GetItemExclusions(bool customTower);

    public List<Pickups> GetItemExclusions(bool customTower)
    {
        var list = orig_GetItemExclusions(customTower);
        foreach (var customVariant in InternalCustomVariants.Values) 
        {
            if (customVariant.Value && customVariant.ItemExclusions != null) 
            {
                list.AddRange(customVariant.ItemExclusions);
            }
        }
        return list;
    }

    public List<VariantItem> BuildMenu(MainMenu menu, out MenuItem startSelected, out float maxUICameraY)
    {
        float yOffset = 50f;
        List<VariantItem> items = new List<VariantItem>();
        List<VariantHeader> headers = new List<VariantHeader>();
        List<List<VariantItem>> grid = new List<List<VariantItem>>();
        List<Variant> notAllowed = new List<Variant>();

        Dictionary<string, int> headerFilter = new Dictionary<string, int>();

        grid.Add(new List<VariantItem>());
        grid[0].Add(new VariantDisableAll());
        grid[0].Add(new VariantRandomize());
        grid[0].Add(new VariantTournament1v1());
        if (GameData.DarkWorldDLC)
        {
            grid[0].Add(new VariantTournament2v2(true));
        }

        foreach (var preset in PresetRegistry.Presets)
        {
            Variant[] variants = new Variant[preset.Configuration.Variants.Length];
            for (int i = 0; i < variants.Length; i += 1)
            {
                var entry = preset.Configuration.Variants[i];
                var variant = InternalCustomVariants[entry.Name];
                variants[i] = variant;
            }
            grid[0].Add(
                new ModVariantPreset(
                    preset.Configuration.Icon, 
                    preset.Configuration.Name.ToUpperInvariant(), 
                    preset.Configuration.Description?.ToUpperInvariant() ?? "",
                    variants
                )
            );
        }

        grid.Add(new List<VariantItem>());
        grid[1].Add(new VariantPreset(0));
        grid[1].Add(new VariantPreset(1));
        grid[1].Add(new VariantPreset(2));
        grid[1].Add(new VariantPreset(3));
        grid[1].Add(new VariantPreset(4));

        foreach (VariantItem variantItem in grid[0])
        {
            items.Add(variantItem);
        }

        foreach (VariantItem variantItem in grid[1])
        {
            items.Add(variantItem);
        }

        int columnIndex = 1;
        foreach (var variant in Variants)
        {
            // if this is not a custom variant, perform the vanilla variant insertion
            if (!InternalCustomVariants.ContainsValue(variant))
            {
                if (!string.IsNullOrEmpty(variant.Header))
                {
                    VariantHeader variantHeader = new VariantHeader(variant.Header);
                    headers.Add(variantHeader);
                    items.Add(variantHeader);
                    grid.Add(new List<VariantItem>());
                    // we do want its index though..
                    columnIndex += 1;
                    headerFilter[variant.Header] = columnIndex;
                }    
            }
            // delay all of the modded variants that had headers since we want all non-headers variant
            // to be place at the top
            else if (!string.IsNullOrEmpty(variant.Header))
            {
                // only do this if the header has not yet been discovered.
                if (!headerFilter.ContainsKey(variant.Header))
                {
                    VariantHeader variantHeader = new VariantHeader(variant.Header);
                    headers.Add(variantHeader);
                    items.Add(variantHeader);
                    grid.Add(new List<VariantItem>());
                    // we do want its index though..
                    columnIndex += 1;
                    headerFilter[variant.Header] = columnIndex;
                }
                continue;
            }

            if (variant.DarkWorldDLC && !GameData.DarkWorldDLC)
            {
                notAllowed.Add(variant);
            }
            else if (variant.VisibleInMenu)
            {
                VariantItem variantItem2 = new VariantToggle(variant, true);
                items.Add(variantItem2);
                grid[columnIndex].Add(variantItem2);
            }
        }

        foreach (var p in InternalCustomVariants)
        {
            var variant = p.Value;
            if (string.IsNullOrEmpty(variant.Header))
            {
                continue;
            }
            var cIndex = headerFilter[variant.Header];
            if (variant.DarkWorldDLC && !GameData.DarkWorldDLC)
            {
                notAllowed.Add(variant);
            }
            else if (variant.VisibleInMenu)
            {
                VariantItem variantItem2 = new VariantToggle(variant, true);
                items.Add(variantItem2);
                // this is why we store the columnIndex
                grid[cIndex].Add(variantItem2);
            }
        }

        if (!GameData.DarkWorldDLC)
        {
            VariantHeader variantHeader = new VariantHeader("DARK WORLD EXPANSION");
            headers.Add(variantHeader);
            items.Add(variantHeader);
            grid.Add(new List<VariantItem>());
            columnIndex += 1;
            VariantTournament2v2 variantTournament2v = new VariantTournament2v2(false);
            grid[columnIndex].Add(variantTournament2v);
            items.Add(variantTournament2v);
            foreach (Variant variant in notAllowed)
            {
                if (variant.VisibleInMenu)
                {
                    VariantItem variantItem2 = new VariantToggle(variant, false);
                    items.Add(variantItem2);
                    grid[columnIndex].Add(variantItem2);
                }
            }
        }

        RiseCore.Events.Invoke_OnSlotVariantCreated(this, grid);

        // this one sets all of the key inputs for variants
        for (int y = 0; y < grid.Count; y++)
        {
            for (int x = 0; x < grid[y].Count; x++)
            {
                if (x > 0)
                {
                    grid[y][x].LeftItem = grid[y][x - 1];
                }
                else if (y > 0)
                {
                    grid[y][x].LeftItem = grid[y - 1][grid[y - 1].Count - 1];
                }
                if (x < grid[y].Count - 1)
                {
                    grid[y][x].RightItem = grid[y][x + 1];
                }
                else if (y < grid.Count - 1)
                {
                    grid[y][x].RightItem = grid[y + 1][0];
                }
                if (x / 7 > 0)
                {
                    grid[y][x].UpItem = grid[y][x - 7];
                }
                else if (y > 0)
                {
                    int num3 = (grid[y - 1].Count - 1) / 7 * 7 + x % 7;
                    num3 = Math.Min(num3, grid[y - 1].Count - 1);
                    grid[y][x].UpItem = grid[y - 1][num3];
                }
                if (grid[y].Count > x / 7 * 7 + 7)
                {
                    grid[y][x].DownItem = grid[y][Math.Min(x + 7, grid[y].Count - 1)];
                }
                else if (y < grid.Count - 1)
                {
                    int num3 = x % 7;
                    num3 = Math.Min(num3, grid[y + 1].Count - 1);
                    grid[y][x].DownItem = grid[y + 1][num3];
                }
            }
        }

        // this one sets all of the positioning of variants and headers
        for (int j = 0; j < grid.Count; j++)
        {
            if (j > 1)
            {
                VariantHeader variantHeader = headers[j - 2];
                variantHeader.Position = new Vector2(70f, yOffset);
                yOffset += 20f;
            }
            for (int k = 0; k < grid[j].Count; k++)
            {
                int num4 = k % 7;
                VariantItem variantItem2 = grid[j][k];
                if (k != 0 && k % 7 == 0)
                {
                    yOffset += 26f;
                }
                variantItem2.Position = new Vector2((float)(70 + num4 * 26), yOffset);
            }
            yOffset += 20f;
        }

        startSelected = grid[0][0];
        maxUICameraY = yOffset - 240f;
        return items;
    }

}