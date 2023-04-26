#pragma warning disable CS0626
#pragma warning disable CS0108

using MonoMod;

namespace TowerFall;

public class patch_DarkWorldLevelSelectOverlay : DarkWorldLevelSelectOverlay
{
    private MapScene map;
    public patch_DarkWorldLevelSelectOverlay(MapScene map) : base(map)
    {
    }

    [MonoModIgnore]
    [MonoModConstructor]
    [PatchDarkWorldLevelSelectOverlayCtor]
    public extern void ctor(MapScene map);
    

    public extern void orig_Update();
    private void CheckUpdate() 
    {
        if (map.Selection is DarkWorldMapButton)
            orig_Update();
    }

    public override void Update()
    {
        CheckUpdate();
    }
}