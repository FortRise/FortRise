using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.VersusMapButton")]
public class VersusMapButton : TowerFall.VersusMapButton
{
    private Image noRandomImage;
    private Wiggler noRandomWiggler;
    private float arrowAlpha;
    private SineWave arrowSine;

    public VersusMapButton(VersusTowerData tower) : base(tower)
    {
    }

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.MapButton", "System.Void .ctor(TowerFall.TowerMapData)")]
    public void base_ctor(TowerMapData data) {}

    [MonoModReplace]
    [MonoModConstructor]
    public void ctor(VersusTowerData tower)
    {
        base_ctor(new TowerMapData(tower));

        noRandomImage = new Image(TFGame.MenuAtlas["towerIcons/noRandom"]);
        noRandomImage.CenterOrigin();
        noRandomImage.Position = new Vector2(12f, 12f);
        noRandomImage.Visible = false;
        Add(noRandomImage);

        noRandomWiggler = Wiggler.Create(30, 4f, null, delegate(float v)
        {
            noRandomImage.Scale = Vector2.One * (1f + 0.2f * v);
        }, false, false);

        Add(noRandomWiggler);
        if (MapScene.NoRandom.Contains(tower.LevelID))
        {
            NoRandom = true;
            noRandomImage.Visible = true;
        }
        CanSetSeed = tower.Procedural;
        if (CanSetSeed)
        {
            arrowAlpha = 0f;
            Add(arrowSine = new SineWave(90));
        }

        DarkWorldDLCLocked = Locked = tower.IsOfficialTowerSet && Data.ID.X >= 12 && !GameData.DarkWorldDLC;
        if (Locked)
        {
            CanSetSeed = false;
        }
    }

    [MonoModReplace]
    public override void AltAction()
    {
        if (NoRandom)
        {
            NoRandom = false;
            noRandomImage.Visible = false;
            Sounds.ui_subclickOn.Play(160f, 1f);
            MapScene.NoRandom.Remove(Data.LevelData.LevelID);
        }
        else
        {
            NoRandom = true;
            noRandomImage.Visible = true;
            noRandomWiggler.Start();
            Sounds.ui_subclickOff.Play(160f, 1f);
            MapScene.NoRandom.Add(Data.LevelData.LevelID);
        }
    }

    protected override bool GetLocked()
    {
        if (Map is null || Map.IsOfficialTowerSet)
        {
            return Locked;
        }
        var id = (Data as patch_TowerMapData).LevelData.GetLevelID();
        var tower = TowerRegistry.VersusTowers[id];
        bool? locked = tower.Configuration.ShowLocked?.Invoke(tower);

        return locked is {} l && l;
    }
}