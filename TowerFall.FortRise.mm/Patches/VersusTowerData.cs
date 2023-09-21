namespace TowerFall;

public class patch_VersusTowerData : VersusTowerData 
{
    public float[] TreasureChances;
}

public static class VersusTowerDataExt 
{
    public static float[] GetTreasureChances(this VersusTowerData data) 
    {
        var cast = ((patch_VersusTowerData)data);
        return cast.TreasureChances ?? TreasureSpawner.DefaultTreasureChances;
    }

    public static void SetTreasureChances(this VersusTowerData data, float[] treasureChance) 
    {
        ((patch_VersusTowerData)data).TreasureChances = treasureChance;
    }
}