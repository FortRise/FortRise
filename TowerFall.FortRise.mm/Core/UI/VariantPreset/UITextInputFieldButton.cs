using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UITextInputFieldButton : MenuItem
{
    public string CurrentInput
    {
        get => string.IsNullOrEmpty(field) ? "ENTER TEXT HERE" : field; 
        set => field = value;
    }


    public UITextInputFieldButton(Vector2 position) : base(position) {}


    public override void Update()
    {
        if (!MainMenu.CanAct)
        {
            return;
        }

        base.Update();
    }

    public override void Render()
    {
        base.Render();
        Color color = Selected ? Color.Yellow : Color.White;
        Draw.TextCentered(TFGame.Font, CurrentInput, Position, color, 1.4f);
    }

    public override void TweenIn() {}
    public override void TweenOut() {}
    protected override void OnConfirm()
    {
        UIInputText inputText = new UIInputText(this, OnInputText, Vector2.Zero) { LayerIndex = 0 };
        Selected = false;
        Scene.Add(inputText);
    }

    protected override void OnDeselect() {}
    protected override void OnSelect() {}

    private void OnInputText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        CurrentInput = input.Trim().ToUpperInvariant();
    }
}
