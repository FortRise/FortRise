using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class AdventureButton : MainModeButton
{
    private Sprite<string> icon;
    private Image glow;
    private SineWave glowSine;
    private float glowLerp;

    public AdventureButton(Vector2 position, Vector2 tweenFrom) 
    : base(position, tweenFrom, "ADVENTURE", "")
    {
        this.icon = TFGame.MenuSpriteData.GetSpriteString("QuestIcon");
        this.icon.Play("idle", false);
        base.Add(this.icon);
        this.glow = new Image(TFGame.MenuAtlas["questModeGlow"], null);
        this.glow.CenterOrigin();
        base.Add(this.glow);
        base.Add(this.glowSine = new SineWave(300));
    }

    public override void Render()
    {
        this.icon.DrawOutline(1);
        base.Render();
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
    }

    protected override void OnSelect()
    {
        this.icon.Play("selected", false);
        base.OnSelect();
    }

    protected override void OnDeselect()
    {
        this.icon.Play("idle", false);
    }

    public override float BaseScale => 1f;

    public override float ImageScale 
    { 
        get => this.icon.Scale.X; 
        set => this.glow.Scale = (this.icon.Scale = Vector2.One * value);
    }
    public override float ImageRotation { get => icon.Rotation; set => icon.Rotation = value; }
    public override float ImageY 
    { 
        get => icon.Y; 
        set 
        {
            this.icon.Y = value;
            this.glow.Y = value;
        }
    }

    protected override void MenuAction()
    {
        MainMenu.CurrentMatchSettings = MainMenu.QuestMatchSettings;
        patch_MainMenu.RollcallMode = patch_MainMenu.patch_RollcallModes.Adventure;
        base.MainMenu.State = MainMenu.MenuState.Rollcall;
    }
}