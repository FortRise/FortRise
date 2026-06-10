using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace FortRise;

public class ColorWheel
{
    private static Dictionary<int, Subtexture> textureSizes = [];

    private readonly Subtexture texture;
    private Vector2 cursorPos;
    private float maxRadius;
    private Color[] colors;
    private int size;
    private Vector2 center;

    public Color SelectedColor { get; private set; } = Color.White;

    public ColorWheel(int size)
    {
        this.size = size;
        float halfSize = size / 2f;

        if (textureSizes.TryGetValue(size, out var tex))
        {
            texture = tex;
        }
        else
        {
            texture = new Subtexture(new Monocle.Texture(GenerateColorWheelTexture(size, out colors)));
        }

        center = new Vector2(halfSize, halfSize);
        maxRadius = halfSize - 1f;
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
                
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                float angle = (float)Math.Atan2(dy, dx);

                if (distance > radius)
                {
                    colors[x + y * size] = Color.Transparent;
                }
                else
                {
                    float hue = angle / (float)(2 * Math.PI);
                    if (hue < 0) hue += 1f;

                    float saturation = distance / radius;
                    float lightness = 1f;

                    Color color = HsvToRgb(hue, saturation, lightness);
                    colors[x + y * size] = color;
                }
            }
        }

        tex.SetData(colors);
        return tex;
    }

    public void Update()
    {
        float dt = Engine.DeltaTime;
        Vector2 movement = Vector2.Zero;

        if (MenuInput.Down) movement.Y += 1;
        if (MenuInput.Up) movement.Y -= 1;
        if (MenuInput.Left) movement.X -= 1;
        if (MenuInput.Right) movement.X += 1;

        if (movement == Vector2.Zero)
        {
            return;
        }

        movement.Normalize();
        cursorPos += movement * 5000 * dt;

        float dist = cursorPos.Length();

        if (dist > maxRadius)
        {
            cursorPos = Vector2.Normalize(cursorPos) * maxRadius;
        }

        UpdateSelectedColor();
    }

    private void UpdateSelectedColor()
    {
        float halfSize = size / 2f;
        float exactX = cursorPos.X + halfSize;
        float exactY = cursorPos.Y + halfSize;

        int textureX = (int)Math.Clamp(Math.Floor(exactX), 0, size - 1);
        int textureY = (int)Math.Clamp(Math.Floor(exactY), 0, size - 1);

        int index = textureX + textureY * size;
        SelectedColor = colors[index];        
    }

    private static Color HsvToRgb(float hue, float saturation, float value)
    {
        int hi = (int)Math.Floor(hue * 6) % 6;
        float f = hue * 6 - (float)Math.Floor(hue * 6);
        float p = value * (1 - saturation);
        float q = value * (1 - f * saturation);
        float t = value * (1 - (1 - f) * saturation);

        float r = 0, g = 0, b = 0;

        switch (hi)
        {
            case 0: r = value; g = t; b = p; break;
            case 1: r = q; g = value; b = p; break;
            case 2: r = p; g = value; b = t; break;
            case 3: r = p; g = q; b = value; break;
            case 4: r = t; g = p; b = value; break;
            case 5: r = value; g = p; b = q; break;
        }

        return new Color(new Vector3(r, g, b));
    }

    public void Render(Vector2 position)
    {
        Draw.OutlineTextureCentered(texture, position, Color.White);

        Vector2 screenCursorPos = position + center + cursorPos - new Vector2(texture.Width / 2, texture.Height / 2);

        Rectangle cursorRect = new Rectangle((int)screenCursorPos.X - 3, (int)screenCursorPos.Y - 3, 4, 4);
        
        Draw.Rect(cursorRect, Color.Black);
        Draw.Rect(new Rectangle((int)screenCursorPos.X - 2, (int)screenCursorPos.Y - 2, 2, 2), SelectedColor);
    }
}