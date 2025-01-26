using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;


public delegate void QuestEventAction(Level level);
public static class QuestEventRegistry 
{
    private static Dictionary<string, Event> events = new Dictionary<string, Event>();

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

    public static void Register(string eventName, QuestEventAction appear, QuestEventAction dieOut)
    {
        events[eventName] = new Event(appear, dieOut);
    }

    public struct Event(QuestEventAction appear, QuestEventAction dieOut)
    {
        public QuestEventAction Appear = appear;
        public QuestEventAction DieOut = dieOut;
    }
}