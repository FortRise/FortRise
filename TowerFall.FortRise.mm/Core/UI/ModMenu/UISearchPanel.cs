using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public sealed class UISearchPanel : MenuItem
{
    private Vector2 tweenFrom;
    private Vector2 tweenTo;

    public Action<string> OnSearched;
    private UIInputText inputText;
    private string currentText;

    public UISearchPanel(Vector2 tweenFrom, Vector2 position) : base(position)
    {
        this.tweenFrom = tweenFrom;
        tweenTo = position;

        inputText = new UIInputText(this, (x) =>
        {
            currentText = x;
            OnSearched?.Invoke(x);
        }, Vector2.Zero)
        {
            LayerIndex = 0
        };
    }

    public override void Render()
    {
        base.Render();
        float y = TFGame.Font.MeasureString("Y").Y;

        Draw.HollowRect(Position.X - 4, Position.Y - 4, 280, y + 8, Selected ? Color.Yellow : Color.White);
        if (!string.IsNullOrEmpty(currentText))
        {
            Draw.TextJustify(TFGame.Font, currentText.ToUpperInvariant(), Position, Color.White, new Vector2(0, 0));
        }
        else
        {
            Draw.TextJustify(TFGame.Font, "SEARCH", Position, Color.DarkGray, new Vector2(0, 0));
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

    protected override void OnConfirm()
    {
        if (Scene is not null)
        {
            Selected = false;
            Scene.Add(inputText);
        }
    }

    protected override void OnDeselect() { }

    protected override void OnSelect() { }
}
