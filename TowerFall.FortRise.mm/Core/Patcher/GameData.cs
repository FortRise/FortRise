using System;
using System.Collections.Generic;
using System.Xml;
using FortRise.Adventure;
using TowerFall;

namespace FortRise;

public partial class RiseCore 
{
    public static class GameData 
    {
        public static IReadOnlyDictionary<string, TowerTheme> Themes => InternalThemes;
        public static IReadOnlyDictionary<string, TilesetData> Tilesets => InternalTilesets;
        public static IReadOnlyDictionary<string, XmlElement> BGs => InternalBGs;
        public static IReadOnlyDictionary<string, MapRendererNode> MapRenderers => InternalMapRenderers;

        internal static Dictionary<string, TowerTheme> InternalThemes = new();
        internal static Dictionary<string, TilesetData> InternalTilesets = new();
        internal static Dictionary<string, XmlElement> InternalBGs = new();
        internal static Dictionary<string, MapRendererNode> InternalMapRenderers = new();

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