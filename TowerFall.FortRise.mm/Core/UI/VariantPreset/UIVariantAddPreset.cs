using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIVariantAddPreset : MenuItem
{
    private UITextInputFieldButton fieldButton;
    private UIColorInputFieldButton colorButton;
    private Action<string, Color> onConfirmed;

    public UIVariantAddPreset(Vector2 position, Action<string, Color> onConfirmed) : base(position)
    {
        this.onConfirmed = onConfirmed;
    }

    public override void Added()
    {
        base.Added();

        fieldButton = new UITextInputFieldButton(new Vector2(320 * 0.5f, 100))
        {
            Selected = true,
        };

        colorButton = new UIColorInputFieldButton(new Vector2(320 * 0.5f, 130));

        fieldButton.DownItem = colorButton;
        colorButton.UpItem = fieldButton;
        colorButton.DownItem = this;

        UpItem = colorButton;

        Scene.Add(fieldButton);
        Scene.Add(colorButton);
    }

    public override void Render()
    {
        Color color = Selected ? Color.Yellow : Color.White;

        base.Render();
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        Draw.OutlineTextCentered(TFGame.Font, "CREATE", new Vector2(320 * 0.5f, 180), color, Color.Black, 1.4f);
    }

    public override void TweenIn() {}

    public override void TweenOut() {}

    protected override void OnConfirm()
    {
        onConfirmed(fieldButton.CurrentInput, colorButton.CurrentInput);
    }

    protected override void OnDeselect() {}

    protected override void OnSelect() {}
}
