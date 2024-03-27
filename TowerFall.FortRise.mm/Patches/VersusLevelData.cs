using MonoMod;

namespace TowerFall;

public class patch_VersusLevelData : VersusLevelData
{
    public patch_VersusLevelData(string path) : base(path)
    {
    }

    public patch_VersusLevelData() : base(null)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}