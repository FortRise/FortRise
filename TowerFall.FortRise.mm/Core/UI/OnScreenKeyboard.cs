using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using TowerFall;
using Gamepad = TowerFall.Patching.XGamepadInput;

namespace FortRise;

public class OnScreenKeyboard : Entity
{
    public const char Backspace = (char)8;
    public const char Enter = (char)10;
    public const char Paste = (char)22;
    public const char Cancel = (char)27;
    private const char Shift = (char)15;

    private const int MinKeyHeight = 12;
    private const int Unit = 13;
    private const int Gap = 1;
    private const int Padding = 4;
    private const int IconSpacing = 3;

    private static readonly Color KeyColor = Calc.HexToColor("2B2B2B");
    private static readonly Color FlashColor = Calc.HexToColor("6B6B6B");

    private class Key(char value, string label, int units = 1, Func<Gamepad, Subtexture> shortcut = null)
    {
        public char Value = value;
        public string Label = label;
        public int Units = units;
        public Func<Gamepad, Subtexture> Shortcut = shortcut;
        public Subtexture Icon;
        public Rectangle Bounds;
    }

    private readonly Key[][] rows = [
        [.. Keys("`1234567890-="), new(Backspace, "DEL", 3, pad => ButtonIcon(pad, Buttons.X))],
        [.. Keys("qwertyuiop[]\\")],
        [.. Keys("asdfghjkl;'"), new(Enter, "DONE", 4, pad => pad.StartIcon)],
        [new(Shift, "SHIFT", 4, pad => ButtonIcon(pad, Buttons.LeftTrigger)), .. Keys("zxcvbnm,./")],
        [new(Cancel, "CANCEL", 5, pad => pad.BackIcon), new(Paste, "PASTE", 3), new(' ', "SPACE", 6, pad => ButtonIcon(pad, Buttons.Y))],
    ];

    private readonly int playerIndex;
    private readonly Action<char> onConfirmed;
    private Point selected;
    private bool shiftToggled;
    private Key flashKey;
    private float flashTimer;
    private int width;
    private int height;
    private int keyHeight = MinKeyHeight;

    public OnScreenKeyboard(Vector2 position, int playerIndex, Action<char> onConfirmed, int layerIndex = 0)
        : base(position, layerIndex)
    {
        this.playerIndex = playerIndex;
        this.onConfirmed = onConfirmed;
    }

    private static Key[] Keys(string characters)
    {
        var keys = new Key[characters.Length];
        for (int i = 0; i < characters.Length; i += 1)
        {
            keys[i] = new Key(characters[i], char.ToUpperInvariant(characters[i]).ToString());
        }
        return keys;
    }

    private static Subtexture ButtonIcon(Gamepad pad, Buttons button)
    {
        return GamepadConfig.GetIcon(pad.AutoButtonSet, button);
    }

    private bool ShiftActive => shiftToggled || AnyGamepad(pad => pad.XGamepad.LeftTriggerCheck(0.5f));

    public override void Added()
    {
        base.Added();
        var gamepad = FindGamepad();

        foreach (var row in rows)
        {
            foreach (var key in row)
            {
                key.Icon = gamepad is not null ? key.Shortcut?.Invoke(gamepad) : null;
                if (key.Icon is not null)
                {
                    keyHeight = Math.Max(keyHeight, key.Icon.Height + 2);
                }
            }
        }

        int[] rowWidths = new int[rows.Length];
        for (int r = 0; r < rows.Length; r += 1)
        {
            int x = 0;
            foreach (var key in rows[r])
            {
                int content = (int)TFGame.Font.MeasureString(key.Label).X;
                if (key.Icon is not null)
                {
                    content += key.Icon.Width + IconSpacing;
                }

                int keyWidth = Math.Max(key.Units * Unit - Gap, content + 6);
                key.Bounds = new Rectangle(x, r * (keyHeight + Gap), keyWidth, keyHeight);
                x += keyWidth + Gap;
            }
            rowWidths[r] = x - Gap;
            width = Math.Max(width, rowWidths[r]);
        }

        height = rows.Length * (keyHeight + Gap) - Gap;

        for (int r = 0; r < rows.Length; r += 1)
        {
            int offset = (width - rowWidths[r]) / 2;
            foreach (var key in rows[r])
            {
                key.Bounds.X += offset;
            }
        }

        X = MathF.Floor(X - width / 2f - Padding);
        Y = MathF.Floor(Y - height - Padding * 2);
    }

    private Gamepad FindGamepad()
    {
        if (playerIndex != -1)
        {
            return TFGame.PlayerInputs[playerIndex] as Gamepad;
        }

        foreach (var input in MenuInput.MenuInputs)
        {
            if (input is Gamepad gamepad)
            {
                return gamepad;
            }
        }

        return null;
    }

    public override void Update()
    {
        base.Update();

        if (flashTimer > 0f)
        {
            flashTimer -= Engine.DeltaTime;
        }

        if (Pressed(i => i.MenuLeft, MenuInput.Left))
        {
            selected.X = (selected.X - 1 + rows[selected.Y].Length) % rows[selected.Y].Length;
        }
        else if (Pressed(i => i.MenuRight, MenuInput.Right))
        {
            selected.X = (selected.X + 1) % rows[selected.Y].Length;
        }
        else if (Pressed(i => i.MenuDown, MenuInput.Down))
        {
            MoveRow(1);
        }
        else if (Pressed(i => i.MenuUp, MenuInput.Up))
        {
            MoveRow(-1);
        }

        if (Pressed(i => i.MenuConfirm, MenuInput.Confirm))
        {
            Press(rows[selected.Y][selected.X]);
        }
        else if (AnyGamepad(pad => pad.XGamepad.Pressed(Buttons.X)))
        {
            Press(Find(Backspace));
        }
        else if (AnyGamepad(pad => pad.XGamepad.Pressed(Buttons.Y)))
        {
            Press(Find(' '));
        }
        else if (AnyGamepad(pad => pad.MenuBack))
        {
            Press(Find(Cancel));
        }
    }

    private void MoveRow(int direction)
    {
        int row = Math.Clamp(selected.Y + direction, 0, rows.Length - 1);
        if (row == selected.Y)
        {
            return;
        }

        // pick the key closest to the current one horizontally
        int center = rows[selected.Y][selected.X].Bounds.Center.X;
        int best = 0;
        for (int i = 1; i < rows[row].Length; i += 1)
        {
            if (Math.Abs(rows[row][i].Bounds.Center.X - center) < Math.Abs(rows[row][best].Bounds.Center.X - center))
            {
                best = i;
            }
        }

        selected = new Point(best, row);
    }

    private Key Find(char value)
    {
        foreach (var row in rows)
        {
            foreach (var key in row)
            {
                if (key.Value == value)
                {
                    return key;
                }
            }
        }
        return null;
    }

    private void Press(Key key)
    {
        flashKey = key;
        flashTimer = 0.1f;

        if (key.Value == Shift)
        {
            shiftToggled = !shiftToggled;
            return;
        }

        char value = key.Value;
        if (char.IsLetter(value) && ShiftActive)
        {
            value = char.ToUpperInvariant(value);
            shiftToggled = false;
        }

        onConfirmed(value);
    }

    private bool Pressed(Func<PlayerInput, bool> player, bool menu)
    {
        return playerIndex == -1 ? menu : player(TFGame.PlayerInputs[playerIndex]);
    }

    private bool AnyGamepad(Func<Gamepad, bool> check)
    {
        if (playerIndex != -1)
        {
            return TFGame.PlayerInputs[playerIndex] is Gamepad input && check(input);
        }

        foreach (var input in MenuInput.MenuInputs)
        {
            if (input is Gamepad gamepad && check(gamepad))
            {
                return true;
            }
        }

        return false;
    }

    public override void Render()
    {
        base.Render();

        var font = TFGame.Font;
        bool shift = ShiftActive;

        for (int r = 0; r < rows.Length; r += 1)
        {
            for (int k = 0; k < rows[r].Length; k += 1)
            {
                var key = rows[r][k];
                bool isSelected = selected.X == k && selected.Y == r;
                bool isFlashing = key == flashKey && flashTimer > 0f;

                float x = X + Padding + key.Bounds.X;
                float y = Y + Padding + key.Bounds.Y;
                float centerY = y + keyHeight / 2f;

                Color fill = isSelected ? Color.White : isFlashing ? FlashColor : KeyColor;
                Color text = isSelected ? Color.Black : key.Value == Shift && shift ? Color.Gold : Color.White;
                Draw.Rect(x, y, key.Bounds.Width, keyHeight, fill);

                float labelWidth = font.MeasureString(key.Label).X;
                float contentWidth = key.Icon is not null ? key.Icon.Width + IconSpacing + labelWidth : labelWidth;
                float contentX = MathF.Floor(x + key.Bounds.Width / 2f - contentWidth / 2f);

                if (key.Icon is not null)
                {
                    Draw.Texture(key.Icon, new Vector2(contentX, MathF.Floor(centerY - key.Icon.Height / 2f)), Color.White);
                    contentX += key.Icon.Width + IconSpacing;
                }

                Draw.TextJustify(font, key.Label, new Vector2(contentX, centerY), text, new Vector2(0f, 0.5f));
            }
        }
    }
}
