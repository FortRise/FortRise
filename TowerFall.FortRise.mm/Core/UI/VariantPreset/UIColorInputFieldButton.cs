using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIColorInputFieldButton : MenuItem
{
    public Color CurrentInput
    {
        get => field; 
        set => field = value;
    } = Color.White;


    public UIColorInputFieldButton(Vector2 position) : base(position) {}

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
        const int Width = 100;
        const int Height = 20;

        base.Render();
        Color color = Selected ? Color.Yellow : Color.White;

        Draw.HollowRect(new Rectangle((int)(Position.X  - Width * 0.5f) - 2, (int)Position.Y - 2, Width + 4, Height + 4), color);
        Draw.Rect(new Rectangle((int)(Position.X  - Width * 0.5f), (int)Position.Y, Width, Height), CurrentInput);
    }

    public override void TweenIn() {}
    public override void TweenOut() {}
    protected override void OnConfirm()
    {
        Scene.Add(new UIInputColor(this, OnInputColor, new Vector2(160, 120)));
    }

    protected override void OnDeselect() {}
    protected override void OnSelect() {}

    private void OnInputColor(Color color)
    {
        CurrentInput = color;
    }
}
