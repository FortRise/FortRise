using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

public class InputOptionsButton : OptionsButton
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "changedWiggler")]
    private static extern ref Wiggler changedWiggler(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "selectedWiggler")]
    private static extern ref Wiggler selectedWiggler(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "wiggleDir")]
    private static extern ref int wiggleDir(OptionsButton target);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "title")]
    private static extern ref string title(OptionsButton target);

    private TowerFall.Patching.XGamepadInput input;
    private Buttons[] buttons;
    private Action<Buttons[]> onInput;

    public Buttons[] Buttons
    {
        get => buttons;
        set => buttons = value;
    }

    public InputOptionsButton(string title, TowerFall.Patching.XGamepadInput input, Buttons[] buttons, Action<Buttons[]> onInput) : base(title)
    {
        this.input = input;
        this.buttons = buttons;
        this.onInput = onInput;
    }

    public override void Update()
    {
        base.Update();
        if (MenuInput.Alt && Selected)
        {
            Selected = false;
            MainMenu.CanAct = false;

            var listener = new GamepadInputListener(this, input, (x) =>
            {
                // delay for 10 frames
                Alarm.Set(this, 10, () =>
                {
                    Selected = true;
                    MainMenu.CanAct = true;
                });

                if (buttons.Contains(x[0]))
                {
                    return;
                }

                int lastIndex = buttons.Length;
                Array.Resize(ref buttons, buttons.Length + 1);
                buttons[lastIndex] = x[0];

                (MainMenu as patch_MainMenu).QueueToApply(title(this), () =>
                {
                    onInput(buttons);
                });

            }) { LayerIndex = 0};

            Scene.Add(listener);
        }
    }

    protected override void OnConfirm()
    {
        Selected = false;
        MainMenu.CanAct = false;
        var listener = new GamepadInputListener(this, input, (x) =>
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

            buttons = x;
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

    [MonoModLinkTo("Monocle.Entity", "Render")]
    [MonoModIgnore]
    public void base_Render() { }

    public override void Render()
    {
        Vector2 middle = new Vector2(30f + 2f * changedWiggler(this).Value * wiggleDir(this), 0f);
        Color color = Selected ? SelectedColor : NotSelectedColor;
        Draw.OutlineTextJustify(TFGame.Font, title(this), Position + new Vector2(-5f, 0f) + new Vector2(5f * selectedWiggler(this).Value, 0f), color, Color.Black, new Vector2(1f, 0.5f), 1f);

        int buttonLen = buttons.Length;

        float gap = (9f * buttonLen + 4f * (buttonLen - 1)) / -2f;
        for (int i = 0; i < buttonLen; i += 1)
        {
            var pos = new Vector2(Position.X + gap + i * 4 + 9f * (i + 0.5f), Position.Y) + middle;
            Draw.OutlineTextureCentered(GamepadConfig.GetIcon(input.AutoButtonSet, buttons[i]), pos, Color.White);
        }
    
        base_Render();
    }
}
