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
        var mask = versusTowerData.ModTreasureMask();
        thisctor(session, mask, versusTowerData.SpecialArrowRate, versusTowerData.ArrowShuffle);
    } 

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Session session, int[] mask, float arrowChance, bool arrowShuffle) 
    {
        var levelSystem = (session.MatchSettings.LevelSystem as VersusLevelSystem);
        float[] newTreasureChances;
        if (levelSystem != null) 
            newTreasureChances = levelSystem.VersusTowerData.GetTreasureChances();
        else 
            newTreasureChances = TreasureSpawner.DefaultTreasureChances;
        
        Session = session;
        Random = new Random();
        Exclusions = Session.MatchSettings.Variants.GetItemExclusions(Session.MatchSettings.LevelSystem.CustomTower);
        if (!GameData.DarkWorldDLC)
        {
            for (int i = 0; i < TreasureSpawner.DarkWorldTreasures.Length; i++)
            {
                if (TreasureSpawner.DarkWorldTreasures[i])
                {
                    this.Exclusions.Add((Pickups)i);
                }
            }
        }
        if (Session.MatchSettings.Variants.IgnoreTowerItemSet)
        {
            TreasureRates = (float[])TreasureSpawner.DefaultTreasureChances.Clone();
        }
        else
        {
            TreasureRates = new float[TreasureSpawner.DefaultTreasureChances.Length];
            for (int j = 0; j < TreasureRates.Length; j++)
            {
                TreasureRates[j] = (float)mask[j] * newTreasureChances[j];
            }
        }
        if (arrowShuffle || this.Session.MatchSettings.Variants.ArrowShuffle)
        {
            for (int k = 1; k <= 9; k++)
            {
                TreasureRates[k] = 0f;
            }
            foreach (Pickups pickups in this.GetArrowShufflePickups())
            {
                TreasureRates[(int)pickups] = newTreasureChances[(int)pickups];
            }
        }
        foreach (Pickups pickup in this.Exclusions)
        {
            TreasureRates[(int)pickup] = 0f;
        }
        float arrowRates = arrowChance;
        if (this.Session.MatchSettings.Variants.IgnoreTowerItemSet)
        {
            arrowRates = 0.6f;
        }
        TreasureSpawner.AdjustTreasureRatesForSpecialArrows(this.TreasureRates, arrowRates);
    }

    internal static void ExtendTreasures() 
    {
        const int GemPickupID = 20;

        var treasureCount = 21 + PickupsRegistry.PickupDatas.Count;
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
        foreach (var pickup in PickupsRegistry.PickupDatas.Values) 
        {
            var id = pickup.ID;
            var chance = pickup.Chance;
            DefaultTreasureChances[(int)id] = chance;
            FullTreasureMask[(int)id] = 1;
        }
    }

    public extern ArrowTypes orig_GetRandomArrowType(bool includeDefaultArrows);

    public ArrowTypes GetRandomArrowType(bool includeDefaultArrows) 
    {
        var arrow = orig_GetRandomArrowType(includeDefaultArrows);
        var list = new List<ArrowTypes> { arrow };
        foreach (var arrowPickup in PickupsRegistry.PickupDatas.Values)
        {
            if (arrowPickup.ArrowType == null)
            {
                continue;
            }
            if (Exclusions.Contains(arrowPickup.ID))
            {
                continue;
            }
            var type = ArrowsRegistry.Types[arrowPickup.ArrowType];
            list.Add(type);
        }
        
        return Random.Choose(list);
    }

    public extern List<Pickups> orig_GetArrowShufflePickups();

    public List<Pickups> GetArrowShufflePickups() 
    {
        var list = orig_GetArrowShufflePickups();
        foreach (var customArrow in PickupsRegistry.ArrowPickups)
        {
            if (Exclusions.Contains(customArrow))
                continue;
            
            list.Add(customArrow);
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