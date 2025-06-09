using System;
using System.Text.Json.Serialization;
using TowerFall;

namespace FortRise;

public sealed partial class FortRiseDarkWorldTowerStats : DarkWorldTowerStats
{
    [JsonInclude]
    public bool CompletedNormal
    {
        get => base.CompletedNormal;
        set => base.CompletedNormal = value;
    }
    [JsonInclude]
    public bool CompletedHardcore
    {
        get => base.CompletedHardcore;
        set => base.CompletedHardcore = value;
    }
    [JsonInclude]
    public bool CompletedLegendary
    {
        get => base.CompletedLegendary;
        set => base.CompletedLegendary = value;
    }
    [JsonInclude]
    public bool EarnedEye
    {
        get => base.EarnedEye;
        set => base.EarnedEye = value;
    }
    [JsonInclude]
    public bool EarnedGoldEye
    {
        get => base.EarnedGoldEye;
        set => base.EarnedGoldEye = value;
    }
    [JsonInclude]
    public long Best1PTime
    {
        get => base.Best1PTime;
        set => base.Best1PTime = value;
    }
    [JsonInclude]
    public long Best2PTime
    {
        get => base.Best2PTime;
        set => base.Best2PTime = value;
    }
    [JsonInclude]
    public long Best3PTime
    {
        get => base.Best3PTime;
        set => base.Best3PTime = value;
    }
    [JsonInclude]
    public long Best4PTime
    {
        get => base.Best4PTime;
        set => base.Best4PTime = value;
    }
    [JsonInclude]
    public int Most1PCurses
    {
        get => base.Most1PCurses;
        set => base.Most1PCurses = value;
    }
    [JsonInclude]
    public int Most2PCurses
    {
        get => base.Most2PCurses;
        set => base.Most2PCurses = value;
    }
    [JsonInclude]
    public int Most3PCurses
    {
        get => base.Most3PCurses;
        set => base.Most3PCurses = value;
    }
    [JsonInclude]
    public int Most4PCurses
    {
        get => base.Most4PCurses;
        set => base.Most4PCurses = value;
    }
    [JsonInclude]
    public ulong Deaths
    {
        get => base.Deaths;
        set => base.Deaths = value;
    }
    [JsonInclude]
    public ulong Attempts
    {
        get => base.Attempts;
        set => base.Attempts = value;
    }
}