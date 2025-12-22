using System;
using MonoMod;

namespace TowerFall;

public class patch_VersusTowerData : VersusTowerData 
{
    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.LevelData", "Author")]
    public string Author;
    public float[] TreasureChances;
}

public static class VersusTowerDataExt 
{
    extension(VersusTowerData data)
    {
        public float[] TreasureChances
        {
            get => ((patch_VersusTowerData)data).TreasureChances ?? TreasureSpawner.DefaultTreasureChances;
            set => ((patch_VersusTowerData)data).TreasureChances = value;
        }
    }

    [Obsolete("Use 'VersusTowerData.TreasureChances' instead")]
    public static float[] GetTreasureChances(this VersusTowerData data)
    {
        return data.TreasureChances;
    }

    [Obsolete("Use 'VersusTowerData.TreasureChances' instead")]
    public static void SetTreasureChances(this VersusTowerData data, float[] treasureChance)
    {
        data.TreasureChances = treasureChance;
    }
}
