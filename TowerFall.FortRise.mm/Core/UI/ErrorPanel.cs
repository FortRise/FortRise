using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

internal class ErrorPanel 
{
    public static HashSet<string> Errors = new HashSet<string>();
    public static bool Show;
    public ErrorPanel()
    {

    }

    internal void Update()
    {
        if (MenuInput.Confirm)
        {
            Show = false;
        }
    }

    internal void Render()
    {
        int width = (int)(Engine.Instance.Screen.Width * Engine.Instance.Screen.Scale);
        int height = (int)(Engine.Instance.Screen.Height * Engine.Instance.Screen.Scale);
        Draw.SpriteBatch.Begin();
        Draw.Rect(10f, 10f, width - 20, height - 20, Color.Black * 0.8f);

        int i = 0;
        foreach (var error in Errors)
        {
            Draw.SpriteBatch.DrawString(Draw.DefaultFont, error, new Vector2(20, height - 92 - 30 * i), Color.White);
            i += 1;
        }
        Draw.SpriteBatch.End();
    }

    public static void StoreError(string errorLog)
    {
        Errors.Add(errorLog);
    }
}