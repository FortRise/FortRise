using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

/// <summary>
/// A quest event delegate used for appearing and disappearing an event.
/// </summary>
/// <param name="level">An in-game <see cref="TowerFall.Level"/> scene</param>
public delegate void QuestEventAction(Level level);

/// <summary>
/// A registry to register a quest event for quest levels.
/// </summary>
public static class QuestEventRegistry 
{
    private static Dictionary<string, Event> events = new Dictionary<string, Event>();

    private static Dictionary<string, IQuestEventEntry> eventEntries = [];

    public static void AddQuestEvent(IQuestEventEntry entry)
    {
        eventEntries[entry.Name] = entry;
    }

#nullable enable
    public static IQuestEventEntry? GetQuestEvent(string id)
    {
        eventEntries.TryGetValue(id, out var entry);
        return entry;
    }
#nullable disable

    /// <summary>
    /// A read-only events map.
    /// </summary>
    public static IReadOnlyDictionary<string, Event> Events => events;

    internal static void LoadAllBuiltinEvents() 
    {
        Register("FortRise/MiasmaWall", 
        (level) => level.Add(new Miasma(Miasma.Modes.CataclysmBoss)), 
        (_) => Engine.Instance.Scene.Layers[0].GetFirst<Miasma>()?.Dissipate());

        Register("FortRise/MiasmaWallMoving",
        (level) => level.Add(new Miasma(Miasma.Modes.TheAmaranthBoss)),
        (_) => Engine.Instance.Scene.Layers[0].GetFirst<Miasma>()?.Dissipate());

        Register("FortRise/MiasmaBottom",
        (level) => level.Add(new BottomMiasma(BottomMiasma.Modes.DreadwoodBoss)),
        (_) => Engine.Instance.Scene.Layers[0].GetFirst<BottomMiasma>()?.Dissipate());
    }

    /// <summary>
    /// Use this function inside of a <see cref="FortRise.FortModule.Initialize"/> to add a quest event to the registry.
    /// </summary>
    /// <param name="eventName">A name of the event</param>
    /// <param name="appear">An appear callback used to make the event appear</param>
    /// <param name="dieOut">A dieOut callback used to make the event disappear</param>
    public static void Register(string eventName, QuestEventAction appear, QuestEventAction dieOut)
    {
        events[eventName] = new Event(appear, dieOut);
    }

    /// <summary>
    /// A struct containing the callbacks to the quest events.
    /// </summary>
    /// <param name="appear">An appear callback used to make the event appear</param>
    /// <param name="dieOut">A dieOut callback used to make the event disappear</param>
    public struct Event(QuestEventAction appear, QuestEventAction dieOut)
    {
        public QuestEventAction Appear = appear;
        public QuestEventAction DieOut = dieOut;
    }
}