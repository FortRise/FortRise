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
    private string initialValue;
    private string value;
    private string renderValue;
    private string oldRenderValue;
    private Action<string> finished;
    private int playerIndex = -1;
    private OnScreenKeyboard keyboard;

    public UIInputText(MenuItem backItem, Action<string> finished, Vector2 position, string initialValue = null)
        : this(backItem, finished, position, -1, initialValue)
    {
    }

    public UIInputText(MenuItem backItem, Action<string> finished, Vector2 position, int playerIndex, string initialValue = null) : base(position)
    {
        this.backItem = backItem;
        this.finished = finished;
        this.playerIndex = playerIndex;
        this.initialValue = initialValue ?? "";
        value = this.initialValue;
        renderValue = value.ToUpperInvariant();
    }

    public override void Added()
    {
        base.Added();
        TextInputEXT.TextInput += HandleKey;
        TextInputEXT.StartTextInput();
        MainMenu.CanAct = false;

        if (AnyGamepad(_ => true))
        {
            Scene.Add(keyboard = new OnScreenKeyboard(new Vector2(320 * 0.5f, 236), playerIndex, OnScreenConfirmed, LayerIndex));
        }
    }

    private void OnScreenConfirmed(char character)
    {
        if (character == OnScreenKeyboard.Cancel)
        {
            Finish(initialValue);
            return;
        }

        HandleKey(character);
    }

    public override void Update()
    {
        base.Update();

        if (MenuInput.Start)
        {
            Finish(value);
        }
    }

    private bool AnyGamepad(Func<MInput.XGamepadData, bool> check)
    {
        if (playerIndex != -1)
        {
            return TFGame.PlayerInputs[playerIndex] is XGamepadInput input && check(input.XGamepad);
        }

        foreach (var input in MenuInput.MenuInputs)
        {
            if (input is XGamepadInput gamepad && check(gamepad.XGamepad))
            {
                return true;
            }
        }

        return false;
    }

    private void Finish(string result)
    {
        Selected = false;
        MainMenu.CanAct = true;
        backItem.Selected = true;
        RemoveSelf();
        finished?.Invoke(result);
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
            value = value[..^1];
            renderValue = renderValue[..^1];
            return;
        }

        if (obj == 10)
        {
            Finish(value);
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
