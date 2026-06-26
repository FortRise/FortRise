using System;
using Microsoft.Extensions.Logging;
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
    private string oldRenderValue;
    private Action<string> finished;
    private int playerIndex = -1;
    private OnScreenKeyboard keyboard;

    public UIInputText(MenuItem backItem, Action<string> finished, Vector2 position, string initialValue = null) : base(position)
    {
        this.backItem = backItem;
        this.finished = finished;
        value = initialValue;
        renderValue = initialValue?.ToUpperInvariant() ?? "";
    }

    public UIInputText(MenuItem backItem, Action<string> finished, Vector2 position, int playerIndex, string initialValue = null) : base(position)
    {
        this.backItem = backItem;
        this.finished = finished;
        value = initialValue;
        renderValue = initialValue?.ToUpperInvariant() ?? "";
        this.playerIndex = playerIndex;
    }

    public override void Added()
    {
        base.Added();
        TextInputEXT.TextInput += HandleKey;
        TextInputEXT.StartTextInput();
        MainMenu.CanAct = false;
        bool hasController = false;
        if (playerIndex == -1)
        {
            foreach (var inp in MenuInput.MenuInputs)
            {
                if (inp is XGamepadInput)
                {
                    hasController = true;
                }
            }
        }
        else
        {
            var input = TFGame.PlayerInputs[playerIndex];
            if (input is XGamepadInput)
            {
                hasController = true;
            }
        }

        if (hasController)
        {
            Scene.Add(keyboard = new OnScreenKeyboard(new Vector2(320 * 0.2f, 160), playerIndex, OnScreenConfirmed, LayerIndex));
        }
    }

    private void OnScreenConfirmed(char character)
    {
        HandleKey(character);
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

        if (obj == 10)
        {
            Selected = false;
            MainMenu.CanAct = true;
            backItem.Selected = true;
            RemoveSelf();
            finished?.Invoke(value);
            return;
        }

        if (obj == 22)
        {
            oldRenderValue = renderValue;
            var text = SDL.SDL_GetClipboardText();
            if (text is null)
            {
                return;
            }
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
        keyboard?.RemoveSelf();
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
        try
        {
            Draw.TextCentered(TFGame.Font, renderValue + "_", new Vector2(320 * 0.5f, 240 * 0.5f), Color.White);
        }
        catch (ArgumentException)
        {
            renderValue = oldRenderValue;
            RiseCore.logger.LogError("An unsupported character tried to be drawn, but failed.");
        }
    }

    public override void TweenIn() {}
    protected override void OnConfirm() {}
    public override void TweenOut() {}
    protected override void OnSelect() {}
}