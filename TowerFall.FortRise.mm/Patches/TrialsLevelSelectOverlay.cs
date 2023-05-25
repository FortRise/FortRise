namespace TowerFall;

public class patch_TrialsLevelSelectOverlay : TrialsLevelSelectOverlay
{
    private MapScene map;
    public patch_TrialsLevelSelectOverlay(MapScene map) : base(map)
    {
    }

    public extern void orig_Update();

    public override void Update()
    {
        if (map.Selection is TrialsMapButton)
            orig_Update();
    }
}