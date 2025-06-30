using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public static class GameModeRegistry 
{
    internal static int ModesCount = 10;
    public static Dictionary<string, Type> GameModesMap = new();
    public static Dictionary<Type, int> GameModeTypes = new();
    public static List<IVersusGameModeEntry> VersusGameModes = new();
    public static Dictionary<string, IVersusGameModeEntry> RegistryVersusGameModes = new();
    public static Dictionary<Modes, IVersusGameModeEntry> ModesToVersusGameMode = new();
    public static Dictionary<string, Modes> NameToModes = new();

    public static Modes GetGameModeModes(string name) 
    {
        return NameToModes[name];
    }

    public static void AddVersusGamemode(IVersusGameModeEntry gameMode)
    {
        RegistryVersusGameModes[gameMode.Name] = gameMode;
    }

#nullable enable
    public static IVersusGameModeEntry? GetVersusGameMode(string id)
    {
        RegistryVersusGameModes.TryGetValue(id, out var entry);
        return entry;
    }
#nullable disable

    public static void Register(IVersusGameModeEntry gameMode)
    {
        Modes mode = gameMode.Modes;
        VersusGameModes.Add(gameMode);
        ModesToVersusGameMode.Add(mode, gameMode);
        NameToModes.Add(gameMode.Name, mode);
    }
}