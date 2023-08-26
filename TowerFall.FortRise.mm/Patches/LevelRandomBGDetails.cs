using System.Xml;
using MonoMod;

namespace TowerFall;

public static class patch_LevelRandomBGDetails 
{
    public static extern int[,] orig_GenerateTileData(bool[,] baseSolids, bool[,] bg, bool[,] finalSolids, XmlElement entities);

    [MonoModLinkTo("TowerFall.LevelRandomBGDetails", "GenCataclysm")]
    [MonoModIgnore]
    public static int[,] Ugliness(bool[,] baseSolids, bool[,] bg, bool[,] finalSolids, XmlElement entities) { return null; }

    [MonoModOriginalName("GenCataclysm")]
    public static int[,] GenerateTileData(bool[,] baseSolids, bool[,] bg, bool[,] finalSolids, XmlElement entities) 
    {
        return Ugliness(baseSolids, bg, finalSolids, entities);
    }

    public static int[,] GenerateTileData(bool[,] baseSolids, bool[,] bg, bool[,] finalSolids, XmlElement entities, TowerTheme theme) 
    {
        return Ugliness(baseSolids, bg, finalSolids, entities);
    }

    [MonoModIgnore]
    [MonoModPublic]
    private static extern bool Empty(int[,] data, int x, int y, int width, int height);

    [MonoModIgnore]
    [MonoModPublic]
    private static extern bool Check(bool[,] data, int x, int y, int width, int height, bool match);
}