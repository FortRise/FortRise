using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public partial class RiseCore 
{
    public static class GameData 
    {
        public static Dictionary<string, TowerTheme> Themes = new();

        private static Queue<Action> delayedActions = new();

        public static void Load() 
        {
            for (int i = 0; i < delayedActions.Count; i++) 
            {
                delayedActions.Dequeue()();
            }
        }

        public static void Defer(Action action) 
        {
            delayedActions.Enqueue(action);
        }
    }
}