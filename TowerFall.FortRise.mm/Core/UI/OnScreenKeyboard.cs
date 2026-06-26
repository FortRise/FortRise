using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class OnScreenKeyboard : Entity
{
    private readonly char[][] CharacterMatrix = [
        ['~', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '=', (char)8],
        ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '[', ']', '\\'],
        ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';', '\'', (char)10],
        ['z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '/'],
        [(char)32, (char)22]
    ];

    private int playerIndex;
    private Point pointerSelect;
    private Action<char> onConfirmed;

    private bool Left
    {
        get
        {
            if (playerIndex == -1)
            {
                return MenuInput.Left;
            }

            var input = TFGame.PlayerInputs[playerIndex];
            return input.MenuLeft;
        }
    }

    private bool Right
    {
        get
        {
            if (playerIndex == -1)
            {
                return MenuInput.Right;
            }

            var input = TFGame.PlayerInputs[playerIndex];
            return input.MenuRight;
        }
    }

    private bool Up
    {
        get
        {
            if (playerIndex == -1)
            {
                return MenuInput.Up;
            }

            var input = TFGame.PlayerInputs[playerIndex];
            return input.MenuUp;
        }
    }

    private bool Down
    {
        get
        {
            if (playerIndex == -1)
            {
                return MenuInput.Down;
            }

            var input = TFGame.PlayerInputs[playerIndex];
            return input.MenuDown;
        }
    }

    private bool Confirm
    {
        get
        {
            if (playerIndex == -1)
            {
                return MenuInput.Confirm;
            }

            var input = TFGame.PlayerInputs[playerIndex];
            return input.MenuConfirm;
        }
    }

    public OnScreenKeyboard(Vector2 position, int playerIndex, Action<char> onConfirmed, int layerIndex = 0)
        : base(position, layerIndex)
    {
        this.playerIndex = playerIndex;
        this.onConfirmed = onConfirmed;
    }

    public override void Update()
    {
        base.Update();
        if (Left)
        {
            pointerSelect.X = Math.Max(pointerSelect.X - 1, 0);
        }
        else if (Right)
        {
            pointerSelect.X = Math.Min(pointerSelect.X + 1, CharacterMatrix[pointerSelect.Y].Length - 1);
        }
        else if (Down)
        {
            pointerSelect.Y = Math.Min(pointerSelect.Y + 1, CharacterMatrix.Length - 1);
            pointerSelect.X = Math.Min(pointerSelect.X, CharacterMatrix[pointerSelect.Y].Length - 1);
        }
        else if (Up)
        {
            pointerSelect.Y = Math.Max(pointerSelect.Y - 1, 0);
        }

        if (Confirm)
        {
            onConfirmed(CharacterMatrix[pointerSelect.Y][pointerSelect.X]);
        }
    }

    public override void Render()
    {
        base.Render();
        for (int i = 0; i < CharacterMatrix.Length; i += 1)
        {
            var characters = CharacterMatrix[i];
            for (int j = 0; j < characters.Length; j += 1)
            {
                int offset = 4 * i;
                Color color = pointerSelect.X == j && pointerSelect.Y == i ? Color.Yellow : Color.White;
                var character = characters[j];

                Vector2 position = new Vector2(Position.X + j * 14, Position.Y + i * 14);
                if (char.IsLetter(character))
                {
                    Draw.Text(TFGame.Font, char.ToUpperInvariant(character).ToString(), new Vector2(position.X + offset, position.Y), color);
                }
                else if (character == (char)8)
                {
                    Draw.Text(TFGame.Font, "<<", new Vector2(position.X + offset, position.Y), color);
                }
                else if (character == (char)10)
                {
                    Draw.Text(TFGame.Font, ">>", new Vector2(position.X + offset, position.Y), color);
                }
                else if (character == (char)32)
                {
                    Draw.Text(TFGame.Font, "_________", new Vector2(position.X + offset + (8 * 6), position.Y), color);
                }
                else if (character == (char)22)
                {
                    Draw.Text(TFGame.Font, "PASTE", new Vector2(position.X + offset + 2, position.Y), color);
                }
                else
                {
                    Draw.Text(TFGame.Font, character.ToString(), new Vector2(position.X + offset, position.Y), color);
                }
            }
        }
    }
}