using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TowerFall;

namespace FortRise.Adventure;

public sealed partial class AdventureWorldStats 
{
    [JsonInclude]
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

public sealed partial class AdventureWorldTowerStats 
{
    [JsonInclude]
    public bool CompletedNormal;
    [JsonInclude]
    public bool CompletedHardcore;
    [JsonInclude]
    public bool CompletedLegendary;
    [JsonInclude]
    public bool EarnedEye;
    [JsonInclude]
    public bool EarnedGoldEye;
    [JsonInclude]
    public long Best1PTime;
    [JsonInclude]
    public long Best2PTime;
    [JsonInclude]
    public long Best3PTime;
    [JsonInclude]
    public long Best4PTime;
    [JsonInclude]
    public int Most1PCurses;
    [JsonInclude]
    public int Most2PCurses;
    [JsonInclude]
    public int Most3PCurses;
    [JsonInclude]
    public int Most4PCurses;
    [JsonInclude]
    public ulong Deaths;
    [JsonInclude]
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