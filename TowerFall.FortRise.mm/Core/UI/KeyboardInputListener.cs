using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using TowerFall;

namespace FortRise;

public class KeyboardInputListener : Entity
{
    private MenuItem backItem;
    private Action<Keys[]> onInput;

    public KeyboardInputListener(MenuItem backItem, Action<Keys[]> onInput)
    {
        this.backItem = backItem;
        this.onInput = onInput;
    }

    public override void Update()
    {
        base.Update();

        if (MInput.Keyboard.GetFirstPressed(out Keys key))
        {
            onInput([key]);
            RemoveSelf();
        }
}

    public override void Render()
    {
        base.Render();
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        Draw.TextCentered(TFGame.Font, "PRESS ANY BUTTON TO MAP", new Vector2(320 * 0.5f, 240 * 0.5f), Color.White);
    }
}