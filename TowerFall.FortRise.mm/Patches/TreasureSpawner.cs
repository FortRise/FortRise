using System;
using System.Collections.Generic;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TreasureSpawner : TreasureSpawner
{
    public static float[][] ChestChances;
    public static float[] DefaultTreasureChances;
    public static int[] FullTreasureMask;
    public static bool[] DarkWorldTreasures;

    public Session Session { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw null; }
    public float[] TreasureRates { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw null; }
    public List<Pickups> Exclusions { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw null; }
    public Random Random { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw null; }


    public patch_TreasureSpawner(Session session, VersusTowerData versusTowerData) : base(session, versusTowerData)
    {
    }

    [MonoModLinkTo("TowerFall.TreasureSpawner", "System.Void .ctor(TowerFall.Session,System.Int32[],System.Single,System.Boolean)")]
    [MonoModIgnore]
    public void thisctor(Session session, int[] mask, float arrowChance, bool arrowShuffle) {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Session session, patch_VersusTowerData versusTowerData) 
    {
        thisctor(session, versusTowerData.TreasureMask, versusTowerData.SpecialArrowRate, versusTowerData.ArrowShuffle);
    }

    private void ResizeIfNeeded(ref int[] mask, ref float[] chances)
    {
        var count = PickupsRegistry.GetAllPickups().Count;
        if (count == 0)
        {
            return; // No need
        }

        int treasureCount = 21 + count;

        if (mask.Length != treasureCount)
        {
            Array.Resize(ref mask, treasureCount);
        }

        if (chances.Length != treasureCount)
        {
            Array.Resize(ref chances, treasureCount);
        }
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Session session, int[] mask, float arrowChance, bool arrowShuffle) 
    {
        var levelSystem = session.MatchSettings.LevelSystem as VersusLevelSystem;
        float[] newTreasureChances;
        if (levelSystem != null)
        {
            newTreasureChances = levelSystem.VersusTowerData.GetTreasureChances();
        }
        else
        {
            newTreasureChances = TreasureSpawner.DefaultTreasureChances;
        }

        ResizeIfNeeded(ref mask, ref newTreasureChances);

        Session = session;
        Random = new Random();
        Exclusions = Session.MatchSettings.Variants.GetItemExclusions(Session.MatchSettings.LevelSystem.CustomTower);
        if (!GameData.DarkWorldDLC)
        {
            for (int i = 0; i < TreasureSpawner.DarkWorldTreasures.Length; i++)
            {
                if (TreasureSpawner.DarkWorldTreasures[i])
                {
                    Exclusions.Add((Pickups)i);
                }
            }
        }
        if (Session.MatchSettings.Variants.IgnoreTowerItemSet)
        {
            TreasureRates = new float[TreasureSpawner.DefaultTreasureChances.Length];
            var clonedMask = new int[mask.Length];
            Array.Fill(clonedMask, 1);
            var patchContext = new VersusTowerTreasurePatchContext(clonedMask);
            
            foreach (var tower in TowerPatchRegistry.Hooks.Values)
            {
                if (tower.Hook.AffectedByIgnoreTowerItemSetVariant)
                {
                    continue;
                }

                if (tower.Hook.TargetTowers.Contains(levelSystem.VersusTowerData.GetLevelID()))
                {
                    tower.Hook.VersusTowerTreasurePatch(patchContext);
                }
            }

            for (int j = 0; j < TreasureRates.Length; j++)
            {
                TreasureRates[j] = clonedMask[j] * newTreasureChances[j];
            }
        }
        else
        {
            TreasureRates = new float[TreasureSpawner.DefaultTreasureChances.Length];
            var patchContext = new VersusTowerTreasurePatchContext(mask);

            foreach (var tower in TowerPatchRegistry.Hooks.Values)
            {
                if (tower.Hook.TargetTowers.Contains(levelSystem.VersusTowerData.GetLevelID()))
                {
                    tower.Hook.VersusTowerTreasurePatch(patchContext);
                }
            }

            for (int j = 0; j < TreasureRates.Length; j++)
            {
                TreasureRates[j] = mask[j] * newTreasureChances[j];
            }
        }

        if (arrowShuffle || Session.MatchSettings.Variants.ArrowShuffle)
        {
            for (int k = 1; k <= 9; k++)
            {
                TreasureRates[k] = 0f;
            }
            foreach (Pickups pickups in GetArrowShufflePickups())
            {
                TreasureRates[(int)pickups] = newTreasureChances[(int)pickups];
            }
        }

        foreach (Pickups pickup in Exclusions)
        {
            TreasureRates[(int)pickup] = 0f;
        }
        float arrowRates = arrowChance;
        if (Session.MatchSettings.Variants.IgnoreTowerItemSet)
        {
            arrowRates = 0.6f;
        }
        AdjustTreasureRatesForSpecialArrows(TreasureRates, arrowRates);
    }

    internal static void ExtendTreasures() 
    {
        const int GemPickupID = 20;
        var allPickups = PickupsRegistry.GetAllPickups();

        var treasureCount = 21 + allPickups.Count;
        // We don't need to resize, if left unchanged
        if (treasureCount == 21)
            return;
        Array.Resize(ref DefaultTreasureChances, treasureCount);
        Array.Resize(ref FullTreasureMask, treasureCount);
        Array.Resize(ref DarkWorldTreasures, treasureCount);
        // We don't want gem to spawn at all
        DefaultTreasureChances[GemPickupID] = 0;
        FullTreasureMask[GemPickupID] = 0;

        // Put every customs pickups including the arrows to be in the treasure pools
        foreach (var pickup in allPickups.Values) 
        {
            var id = pickup.Pickups;
            var chance = pickup.Configuration.Chance;
            DefaultTreasureChances[(int)id] = chance;
            FullTreasureMask[(int)id] = 1;
        }
    }

    public extern ArrowTypes orig_GetRandomArrowType(bool includeDefaultArrows);

    public ArrowTypes GetRandomArrowType(bool includeDefaultArrows) 
    {
        var arrow = orig_GetRandomArrowType(includeDefaultArrows);
        var list = new List<ArrowTypes> { arrow };
        foreach (var arrowPickup in PickupsRegistry.GetAllPickups().Values)
        {
            if (!arrowPickup.Configuration.ArrowType.TryGetValue(out var type))
            {
                continue;
            }
            if (Exclusions.Contains(arrowPickup.Pickups))
            {
                continue;
            }

            list.Add(type);
        }
        
        return Random.Choose(list);
    }

    public extern List<Pickups> orig_GetArrowShufflePickups();

    public List<Pickups> GetArrowShufflePickups() 
    {
        var list = orig_GetArrowShufflePickups();
        foreach (var customArrow in PickupsRegistry.GetAllPickups().Values)
        {
            if (!customArrow.Configuration.ArrowType.HasValue)
            {
                continue;
            }

            if (Exclusions.Contains(customArrow.Pickups))
            {
                continue;
            }
            
            list.Add(customArrow.Pickups);
        }
        list.Shuffle();
        return list;
    }

    [MonoModReplace]
    public void LogSpawnRates()
    {
        float allRates = 0f;
        for (int i = 0; i < TreasureRates.Length; i++)
        {
            allRates += TreasureRates[i];
        }
        for (int i = 0; i < TreasureRates.Length; i++)
        {
            if (TreasureRates[i] > 0f)
            {
                string name = PickupsRegistry.TypesToString((Pickups)i);
                Engine.Instance.Commands.Log(name + " - " + (this.TreasureRates[i] / allRates * 100f).ToString("F") + "%");
            }
        }
    }

}