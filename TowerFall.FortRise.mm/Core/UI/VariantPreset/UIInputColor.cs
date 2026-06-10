using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIInputColor : MenuItem
{
    private ColorWheel wheel;
    private MenuItem backItem;
    private Action<Color> finished;

    public UIInputColor(MenuItem backItem, Action<Color> finished, Vector2 position) : base(position)
    {
        wheel = new ColorWheel(64);
        this.backItem = backItem;
        this.finished = finished;
    }

    public override void Added()
    {
        base.Added();
        MainMenu.CanAct = false;
    }

    public override void Update()
    {
        base.Update();

        if (MenuInput.Start || MenuInput.Confirm)
        {
            Selected = false;
            MainMenu.CanAct = true;
            backItem.Selected = true;
            RemoveSelf();
            finished?.Invoke(wheel.SelectedColor);
        }
        else
        {
            wheel.Update();
        }
    }

    public override void Render()
    {
        base.Render();
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        wheel.Render(Position);
    }

    public override void TweenIn()
    {
    }

    public override void TweenOut()
    {
    }

    protected override void OnConfirm()
    {
    }

    protected override void OnDeselect()
    {
    }

    protected override void OnSelect()
    {
    }
}
