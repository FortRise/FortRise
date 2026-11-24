using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace FortRise;

public class ColorWheel
{
    private const float CursorSpeed = 40f;
    private const float BrightnessSpeed = 1f;
    private const int SliderWidth = 6;

    private static readonly Dictionary<int, (Subtexture Texture, Color[] Colors)> cache = [];

    private readonly Subtexture texture;
    private readonly Color[] colors;
    private readonly int size;
    private readonly float maxRadius;
    private Vector2 cursorPos;
    private float brightness = 1f;

    public Color SelectedColor { get; private set; } = Color.White;

    public ColorWheel(int size)
    {
        this.size = size;
        maxRadius = size / 2f - 1f;

        if (!cache.TryGetValue(size, out var wheel))
        {
            var generated = GenerateColorWheelTexture(size, out var generatedColors);
            wheel = (new Subtexture(new Monocle.Texture(generated)), generatedColors);
            cache[size] = wheel;
        }

        (texture, colors) = wheel;
    }

    private static Texture2D GenerateColorWheelTexture(int size, out Color[] colors)
    {
        Texture2D tex = new Texture2D(Engine.Instance.GraphicsDevice, size, size);
        colors = new Color[size * size];

        int center = size / 2;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance > radius)
                {
                    colors[x + y * size] = Color.Transparent;
                    continue;
                }

                float hue = MathF.Atan2(dy, dx) / MathF.Tau;
                if (hue < 0)
                {
                    hue += 1f;
                }

                colors[x + y * size] = HsvToRgb(hue, distance / radius, 1f);
            }
        }

        tex.SetData(colors);
        return tex;
    }

    public void Update()
    {
        Vector2 movement = Vector2.Zero;

        if (MenuInput.DownCheck) movement.Y += 1;
        if (MenuInput.UpCheck) movement.Y -= 1;
        if (MenuInput.LeftCheck) movement.X -= 1;
        if (MenuInput.RightCheck) movement.X += 1;

        if (movement != Vector2.Zero)
        {
            cursorPos += Vector2.Normalize(movement) * CursorSpeed * Engine.DeltaTime;

            if (cursorPos.Length() > maxRadius)
            {
                cursorPos = Vector2.Normalize(cursorPos) * maxRadius;
            }
        }

        if (MenuInput.Alt2Check)
        {
            brightness = Math.Min(brightness + BrightnessSpeed * Engine.DeltaTime, 1f);
        }
        else if (MenuInput.AltCheck)
        {
            brightness = Math.Max(brightness - BrightnessSpeed * Engine.DeltaTime, 0f);
        }

        SelectedColor = Color.Lerp(Color.Black, GetHueColor(), brightness);
    }

    private Color GetHueColor()
    {
        float halfSize = size / 2f;
        int x = (int)Math.Clamp(MathF.Floor(cursorPos.X + halfSize), 0, size - 1);
        int y = (int)Math.Clamp(MathF.Floor(cursorPos.Y + halfSize), 0, size - 1);

        return colors[x + y * size];
    }

    private static Color HsvToRgb(float hue, float saturation, float value)
    {
        int hi = (int)MathF.Floor(hue * 6) % 6;
        float f = hue * 6 - MathF.Floor(hue * 6);
        float p = value * (1 - saturation);
        float q = value * (1 - f * saturation);
        float t = value * (1 - (1 - f) * saturation);

        return hi switch
        {
            0 => new Color(value, t, p),
            1 => new Color(q, value, p),
            2 => new Color(p, value, t),
            3 => new Color(p, q, value),
            4 => new Color(t, p, value),
            _ => new Color(value, p, q),
        };
    }

    public void Render(Vector2 position)
    {
        Draw.OutlineTextureCentered(texture, position, Color.Lerp(Color.Black, Color.White, brightness));

        float cursorX = MathF.Floor(position.X + cursorPos.X);
        float cursorY = MathF.Floor(position.Y + cursorPos.Y);
        Draw.Rect(cursorX - 2, cursorY - 2, 5, 5, Color.Black);
        Draw.Rect(cursorX - 1, cursorY - 1, 3, 3, SelectedColor);

        float sliderX = MathF.Floor(position.X + size / 2f + 8);
        float sliderTop = MathF.Floor(position.Y - size / 2f);
        RenderBrightnessSlider(sliderX, sliderTop);

        float previewX = sliderX + SliderWidth + 8;
        float previewY = MathF.Floor(position.Y - 6);
        Draw.Rect(previewX - 1, previewY - 1, 14, 14, Color.Black);
        Draw.Rect(previewX, previewY, 12, 12, SelectedColor);
    }

    private void RenderBrightnessSlider(float x, float top)
    {
        Color hue = GetHueColor();

        Draw.Rect(x - 1, top - 1, SliderWidth + 2, size + 2, Color.Black);
        for (int i = 0; i < size; i += 1)
        {
            Draw.Rect(x, top + i, SliderWidth, 1, Color.Lerp(hue, Color.Black, i / (size - 1f)));
        }

        float markerY = MathF.Floor(top + (1f - brightness) * (size - 1));
        Draw.Rect(x - 2, markerY - 1, SliderWidth + 4, 3, Color.Black);
        Draw.Rect(x - 1, markerY, SliderWidth + 2, 1, Color.White);

        if (GetMenuInput() is { } input)
        {
            Draw.TextureCentered(input.Alt2Icon, new Vector2(x + SliderWidth / 2f, top - 9), Color.White);
            Draw.TextureCentered(input.AltIcon, new Vector2(x + SliderWidth / 2f, top + size + 9), Color.White);
        }
    }

    private static PlayerInput GetMenuInput()
    {
        foreach (var input in MenuInput.MenuInputs)
        {
            if (input is { Attached: true })
            {
                return input;
            }
        }

        return null;
    }
}
