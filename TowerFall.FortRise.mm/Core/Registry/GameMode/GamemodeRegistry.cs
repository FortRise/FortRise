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

    [Obsolete]
    public static bool TryGetGameMode(string name, out IVersusGameModeEntry mode) 
    {
        if (GameModesMap.TryGetValue(name, out var type)) 
        {
            if (GameModeTypes.TryGetValue(type, out var idx)) 
            {
                mode = VersusGameModes[idx];
                return true;
            }
        }
        mode = null;
        return false;
    }

    public static Modes GetGameModeModes(string name) 
    {
        return NameToModes[name];
    }

    public static void Register<T>(FortModule module) 
    {
        Register(typeof(T), module);
    }

    public static void Register(Type type, FortModule module) 
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (type.IsSubclassOf(typeof(CustomGameMode)) && type.IsPublic) 
        {
            var instance = Activator.CreateInstance(type) as CustomGameMode;
            instance.Initialize();
            instance.coinSprite = instance.CoinSprite();
            Register(new VersusGameModeEntry("legacy/" + instance.Name, EnumPool.Obtain<Modes>(), instance));
            LegacyGameModes.Add(instance);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static void Register(IVersusGameModeEntry gameMode)
    {
        Modes mode = gameMode.Modes;
        VersusGameModes.Add(gameMode);
        RegistryVersusGameModes.Add(gameMode.Name, gameMode);
        ModesToVersusGameMode.Add(mode, gameMode);
        NameToModes.Add(gameMode.Name, mode);
    }


#pragma warning disable CS0618 // Type or member is obsolete
    public static List<CustomGameMode> LegacyGameModes = new();
#pragma warning restore CS0618 // Type or member is obsolete
}