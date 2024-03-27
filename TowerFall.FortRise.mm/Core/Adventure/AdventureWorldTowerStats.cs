using System;
using System.Collections.Generic;
using TeuJson;
using TeuJson.Attributes;
using TowerFall;

namespace FortRise.Adventure;

public sealed partial class AdventureWorldStats : IDeserialize, ISerialize 
{
    [TeuObject]
    public Dictionary<string, AdventureWorldTowerStats> Towers = new Dictionary<string, AdventureWorldTowerStats>();

    public AdventureWorldTowerStats AddOrGet(string name) 
    {
        if (Towers.TryGetValue(name, out AdventureWorldTowerStats stats)) 
        {
            return stats;
        }
        var newStats = new AdventureWorldTowerStats();
        Towers.Add(name, newStats);
        return newStats;
    }
}

public sealed partial class AdventureWorldTowerStats : IDeserialize, ISerialize
{
    [TeuObject]
    public bool CompletedNormal;
    [TeuObject]
    public bool CompletedHardcore;
    [TeuObject]
    public bool CompletedLegendary;
    [TeuObject]
    public bool EarnedEye;
    [TeuObject]
    public bool EarnedGoldEye;
    [TeuObject]
    public long Best1PTime;
    [TeuObject]
    public long Best2PTime;
    [TeuObject]
    public long Best3PTime;
    [TeuObject]
    public long Best4PTime;
    [TeuObject]
    public int Most1PCurses;
    [TeuObject]
    public int Most2PCurses;
    [TeuObject]
    public int Most3PCurses;
    [TeuObject]
    public int Most4PCurses;
    [TeuObject]
    public ulong Deaths;
    [TeuObject]
    public ulong Attempts;

    public void Complete(DarkWorldDifficulties difficulty, int players, long time, int continues, int deaths, int curses)
    {
        if (difficulty >= DarkWorldDifficulties.Normal)
        {
            CompletedNormal = true;
        }
        if (difficulty >= DarkWorldDifficulties.Hardcore)
        {
            CompletedHardcore = true;
        }
        if (difficulty >= DarkWorldDifficulties.Legendary)
        {
            CompletedLegendary = true;
            if (continues <= 0)
            {
                EarnedEye = true;
            }
            if (deaths <= 0)
            {
                EarnedGoldEye = true;
            }
        }
        if (difficulty != DarkWorldDifficulties.Legendary)
        {
            return;
        }
        switch (players)
        {
            default:
                if (Best1PTime > 0)
                {
                    Best1PTime = Math.Min(Best1PTime, time);
                }
                else
                {
                    Best1PTime = time;
                }
                Most1PCurses = Math.Max(Most1PCurses, curses);
                break;
            case 2:
                if (Best2PTime > 0)
                {
                    Best2PTime = Math.Min(Best2PTime, time);
                }
                else
                {
                    Best2PTime = time;
                }
                Most2PCurses = Math.Max(Most2PCurses, curses);
                break;
            case 3:
                if (Best3PTime > 0)
                {
                    Best3PTime = Math.Min(Best3PTime, time);
                }
                else
                {
                    Best3PTime = time;
                }
                Most3PCurses = Math.Max(Most3PCurses, curses);
                break;
            case 4:
                if (Best4PTime > 0)
                {
                    Best4PTime = Math.Min(Best4PTime, time);
                }
                else
                {
                    Best4PTime = time;
                }
                Most4PCurses = Math.Max(Most4PCurses, curses);
                break;
        }
    }
}