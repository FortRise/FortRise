using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using SDL3;
using TowerFall;

namespace FortRise;

public class UIInputText : MenuItem
{
    private MenuItem backItem;
    private string value;
    private string renderValue;
    private Action<string> finished;

    public UIInputText(MenuItem backItem, Action<string> finished, Vector2 position, string initialValue = null) : base(position)
    {
        this.backItem = backItem;
        this.finished = finished;
        value = initialValue;
        renderValue = initialValue?.ToUpperInvariant() ?? "";
    }

    public override void Added()
    {
        base.Added();
        TextInputEXT.TextInput += HandleKey;
        TextInputEXT.StartTextInput();
        MainMenu.CanAct = false;
    }

    public override void Update()
    {
        base.Update();

        if (MenuInput.Start)
        {
            Selected = false;
            MainMenu.CanAct = true;
            backItem.Selected = true;
            RemoveSelf();
            finished?.Invoke(value);
        }
    }

    private void HandleKey(char obj)
    {
        if (obj == '\t')
        {
            return;
        }

        if (obj == 8)
        {
            if (value.Length == 0)
            {
                return;
            }
            value = value[0..(value.Length - 1)];
            renderValue = renderValue[0..(renderValue.Length - 1)];
            return;
        }

        if (obj == 22)
        {
            var text = SDL.SDL_GetClipboardText();
            value += text;
            renderValue += text.ToUpperInvariant();
            return;
        }

        if (obj == 127)
        {
            value = "";
            renderValue = "";
            return;
        }

        if (obj > 31 && obj < 127)
        {
            value += obj;
            renderValue += char.ToUpperInvariant(obj);
        }
    }

    public override void Removed()
    {
        base.Removed();
        TextInputEXT.TextInput -= HandleKey;
        TextInputEXT.StopTextInput();
    }

    protected override void OnDeselect() 
    {
    }


    public override void Render()
    {
        base.Render();
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        Draw.TextCentered(TFGame.Font, renderValue + "_", new Vector2(320 * 0.5f, 240 * 0.5f), Color.White);
    }

    public override void TweenIn() {}
    protected override void OnConfirm() {}
    public override void TweenOut() {}
    protected override void OnSelect() {}
}