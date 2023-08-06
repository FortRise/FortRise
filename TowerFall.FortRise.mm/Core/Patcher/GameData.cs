using System;
using System.Collections.Generic;
using System.Xml;
using TowerFall;

namespace FortRise;

public partial class RiseCore 
{
    public static class GameData 
    {
        public static Dictionary<string, TowerTheme> Themes = new();
        public static Dictionary<string, TilesetData> Tilesets = new();
        public static Dictionary<string, XmlElement> BGs = new();

        private static Queue<Action> themesActions = new();
        private static Queue<Action> tilesetActions = new();

        public static void Load() 
        {
            int length = tilesetActions.Count;
            for (int i = 0; i < length; i++) 
            {
                tilesetActions.Dequeue()();
            }
            length = themesActions.Count;
            for (int i = 0; i < length; i++) 
            {
                themesActions.Dequeue()();
            }
        }

        public static void Defer(Action action, int index) 
        {
            switch (index) 
            {
            case 0:
                themesActions.Enqueue(action);
                break;
            case 1:
                tilesetActions.Enqueue(action);
                break;
            }
        }
    }
}