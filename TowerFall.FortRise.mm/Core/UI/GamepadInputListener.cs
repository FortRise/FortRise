using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using TowerFall;

namespace FortRise;

public class GamepadInputListener : Entity
{
    private MenuItem backItem;
    private TowerFall.Patching.XGamepadInput input;
    private Action<Buttons[]> onInput;

    public GamepadInputListener(MenuItem backItem, TowerFall.Patching.XGamepadInput input, Action<Buttons[]> onInput)
    {
        this.backItem = backItem;
        this.input = input;
        this.onInput = onInput;
    }

    public override void Update()
    {
        base.Update();

        var gamepad = MInput.XGamepads[input.XGamepadIndex];

        if (gamepad.Attached)
        {
            if (gamepad.Pressed(Buttons.A)) { onInput([Buttons.A]); }
            else if (gamepad.Pressed(Buttons.B)) { onInput([Buttons.B]); }
            else if (gamepad.Pressed(Buttons.X)) { onInput([Buttons.X]); }
            else if (gamepad.Pressed(Buttons.Y)) { onInput([Buttons.Y]); }
            else if (gamepad.Pressed(Buttons.Start)) { onInput([Buttons.Start]); }
            else if (gamepad.Pressed(Buttons.RightShoulder)) { onInput([Buttons.RightShoulder]); }
            else if (gamepad.Pressed(Buttons.RightTrigger)) { onInput([Buttons.RightTrigger]); }
            else if (gamepad.Pressed(Buttons.LeftShoulder)) { onInput([Buttons.LeftShoulder]); }
            else if (gamepad.Pressed(Buttons.LeftTrigger)) { onInput([Buttons.LeftTrigger]); }

            else if (gamepad.Pressed(Buttons.Paddle1EXT)) { onInput([Buttons.Paddle1EXT]); }
            else if (gamepad.Pressed(Buttons.Paddle2EXT)) { onInput([Buttons.Paddle2EXT]); }
            else if (gamepad.Pressed(Buttons.Paddle3EXT)) { onInput([Buttons.Paddle3EXT]); }
            else if (gamepad.Pressed(Buttons.Paddle4EXT)) { onInput([Buttons.Paddle4EXT]); }

            else { return; }
        }

        RemoveSelf();
}

    public override void Render()
    {
        base.Render();
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        Draw.TextCentered(TFGame.Font, "PRESS ANY BUTTON TO MAP", new Vector2(320 * 0.5f, 240 * 0.5f), Color.White);
    }
}