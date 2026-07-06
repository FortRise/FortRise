using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

public class KeyboardInputOptionsButton : OptionsButton
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "changedWiggler")]
    private static extern ref Wiggler changedWiggler(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "selectedWiggler")]
    private static extern ref Wiggler selectedWiggler(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "wiggleDir")]
    private static extern ref int wiggleDir(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "title")]
    private static extern ref string title(OptionsButton target);

    private Keys[] keys;
    private Action<Keys[]> onInput;

    public Keys[] Buttons
    {
        get => keys;
        set => keys = value;
    }

    public KeyboardInputOptionsButton(string title, Keys[] buttons, Action<Keys[]> onInput) : base(title)
    {
        this.keys = buttons;
        this.onInput = onInput;
    }

    public override void Update()
    {
        base.Update();
        if (MenuInput.Alt && Selected)
        {
            Selected = false;
            MainMenu.CanAct = false;

            var listener = new KeyboardInputListener(this, (x) =>
            {
                // delay for 10 frames
                Alarm.Set(this, 10, () =>
                {
                    Selected = true;
                    MainMenu.CanAct = true;
                });

                if (keys.Contains(x[0]))
                {
                    return;
                }

                int lastIndex = keys.Length;
                Array.Resize(ref keys, keys.Length + 1);
                keys[lastIndex] = x[0];

                (MainMenu as patch_MainMenu).QueueToApply(title(this), () =>
                {
                    onInput(keys);
                });

            }) { LayerIndex = 0};

            Scene.Add(listener);
        }
    }

    [MonoModLinkTo("Monocle.Entity", "Render")]
    [MonoModIgnore]
    public void base_Render() { }

    protected override void OnConfirm()
    {
        Selected = false;
        MainMenu.CanAct = false;
        var listener = new KeyboardInputListener(this, (x) =>
        {
            // delay for 10 frames
            Alarm.Set(this, 10, () =>
            {
                Selected = true;
                MainMenu.CanAct = true;
            });

            (MainMenu as patch_MainMenu).QueueToApply(title(this), () =>
            {
                onInput(x);
            });

            keys = x;
        }) { LayerIndex = 0};

        Scene.Add(listener);
    }

    protected override void OnSelect()
    {
        base.OnSelect();
        MainMenu.ButtonGuideA.SetDetails(MenuButtonGuide.ButtonModes.Alt, "ADD BUTTON");
        MainMenu.ButtonGuideB.SetDetails(MenuButtonGuide.ButtonModes.Confirm, "REPLACE BUTTON");
    }

    protected override void OnDeselect()
    {
        base.OnDeselect();
        MainMenu.ButtonGuideA.Clear();
        MainMenu.ButtonGuideB.Clear();
    }

    public override void Render()
    {
        Vector2 middle = new Vector2(30f + 2f * changedWiggler(this).Value * wiggleDir(this), 0f);
        Color color = Selected ? SelectedColor : NotSelectedColor;
        Draw.OutlineTextJustify(TFGame.Font, title(this), Position + new Vector2(-5f, 0f) + new Vector2(5f * selectedWiggler(this).Value, 0f), color, Color.Black, new Vector2(1f, 0.5f), 1f);

        int buttonLen = keys.Length;

        float gap = (9f * buttonLen + 4f * (buttonLen - 1)) / -2f;
        for (int i = 0; i < buttonLen; i += 1)
        {
            var pos = new Vector2(Position.X + gap + i * 4 + 9f * (i + 0.5f), Position.Y) + middle;

            for (int j = 0; j < TFGame.PlayerInputs.Length; j += 1)
            {
                var input = TFGame.PlayerInputs[j];
                if (input is KeyboardInput keyboard)
                {
                    Draw.OutlineTextureCentered(KeyboardConfig.GetIcon(keys[i]), pos, Color.White);
                    break;
                }
            }

        }
    
        base_Render();
    }
}
