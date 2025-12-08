using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

internal class UIModListPanel : MenuItem
{
    private Vector2 tweenFrom;
    private Vector2 tweenTo;
    public Action OnConfirmed;

    public UIModListPanel(Vector2 tweenFrom) : base(new Vector2(160f, 120f))
    {
        this.tweenFrom = tweenFrom;
        tweenTo = new Vector2(160f, 120f);
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

    protected override void OnConfirm()
    {
        OnConfirmed?.Invoke();
    }

    protected override void OnDeselect() {}

    protected override void OnSelect() {}

    public override void Render()
    {
        Vector2 justifyWorld = Position - new Vector2(160f, 120f);
        base.Render();

        DrawRectText("TOGGLE MODS", 0);

        void DrawRectText(string text, int offset)
        {
            Color color = Selected ? Color.Yellow : Color.White;
            Vector2 measuredText = TFGame.Font.MeasureString(text);
            Draw.HollowRect(justifyWorld.X + 20 + offset - 4,  justifyWorld.Y + 20 - 4, measuredText.X + 8, measuredText.Y + 8, color);
            Draw.TextJustify(TFGame.Font, text, justifyWorld + new Vector2(20 + offset, 20), color, new Vector2(0, 0));
        }
    }
}
