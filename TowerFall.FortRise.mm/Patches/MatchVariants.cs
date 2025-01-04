#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using FortRise;
using MonoMod;

namespace TowerFall;

public class patch_MatchVariants : MatchVariants 
{
    internal Dictionary<string, Variant> InternalCustomVariants;
    public IReadOnlyDictionary<string, Variant> CustomVariants => InternalCustomVariants;


    private List<Variant> canRandoms;
    [MonoModIgnore]
    public static int Count { get; private set; }
    internal VariantManager manager;

    public void orig_ctor(bool noPerPlayer = false) {}

    [MonoModConstructor]
    public void ctor(bool noPerPlayer = false) 
    {
        InternalCustomVariants = new();

        orig_ctor(noPerPlayer);

        TempVariantHolder.TempCustom = new Dictionary<string, bool>();
        manager = new VariantManager(this);
        foreach (var mod in RiseCore.InternalFortModules) 
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
        Count = Variants.Length;
        manager.Dispose();
    }

    public Variant GetCustomVariant(string name) 
    {
        return InternalCustomVariants[name];
    }

    public extern ArrowTypes orig_GetStartArrowType(int playerIndex, ArrowTypes randomType);

    public ArrowTypes GetStartArrowType(int playerIndex, ArrowTypes randomType) 
    {
        foreach (var (arrowVariant, arrowTypes) in VariantManager.StartWithVariants)
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
}