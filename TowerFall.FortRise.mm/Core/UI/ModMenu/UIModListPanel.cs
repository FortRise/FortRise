using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

internal class UIModListPanel : MenuItem
{
    private Vector2 tweenFrom;
    private Vector2 tweenTo;

    public UIModListPanel(Vector2 tweenFrom) : base(new Vector2(160f, 120f))
    {
        this.tweenFrom = tweenFrom;
        tweenTo = new Vector2(160f, 120f);
    }

    [MonoModLinkTo("Monocle.Entity", "Update")]
    public void base_Update() {}

    public override void Update()
    {
        base_Update();
        if (!Selected)
        {
            return;
        }

        if (MenuInput.Down)
        {
            DownItem?.Selected = true;
            Selected = false;
        }
    }

    public override void TweenIn()
    {
        Position = tweenFrom;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(tweenFrom, tweenTo, t.Eased);
        };
        Add(tween);
    }

    public override void TweenOut()
    {
        Vector2 start = Position;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 12, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(start, tweenFrom, t.Eased);
        };
        Add(tween);
    }

    protected override void OnConfirm() {}

    protected override void OnDeselect() {}

    protected override void OnSelect() {}

    public override void Render()
    {
        Vector2 justifyWorld = Position - new Vector2(160f, 120f);
        base.Render();

        DrawRectText("INSTALLED", 0);
        DrawRectText("BROWSE", 60);

        float y = TFGame.Font.MeasureString("Y").Y;

        Draw.HollowRect(justifyWorld.X + 20 - 4, justifyWorld.Y + 40 - 4, 280, y + 8, Color.White);
        Draw.TextJustify(TFGame.Font, "SEARCH", justifyWorld + new Vector2(20, 40), Color.DarkGray, new Vector2(0, 0));

        void DrawRectText(string text, int offset)
        {
            Vector2 measuredText = TFGame.Font.MeasureString(text);
            Draw.HollowRect(justifyWorld.X + 20 + offset - 4,  justifyWorld.Y + 20 - 4, measuredText.X + 8, measuredText.Y + 8, Color.White);
            Draw.TextJustify(TFGame.Font, text, justifyWorld + new Vector2(20 + offset, 20), Color.White, new Vector2(0, 0));
        }
    }
}
