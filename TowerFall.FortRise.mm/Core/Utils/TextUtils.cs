using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using TowerFall;

namespace FortRise;

public static class TextUtils 
{
    private enum RichTextType { Text, Image }
    private struct RichTextComponent(RichTextType type, string data, int line)
    {
        public string Data => data;
        public int Line => line;
        public RichTextType Type => type;
    }

    private static List<RichTextComponent> components = new List<RichTextComponent>();

    public static void RenderRichText(SpriteFont spriteFont, string text, Vector2 position, Color color)
    {
        ParseText(text);
        float xOffset = 0;
        for (int i = 0; i < components.Count; i++)
        {
            var comp = components[i];
            switch (comp.Type)
            {
                case RichTextType.Text:
                    Draw.SpriteBatch.DrawString(spriteFont, comp.Data, 
                        new Vector2(position.X + xOffset, position.Y), color, 0f, 
                        Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    xOffset += spriteFont.MeasureString(comp.Data).X;
                    break;
                case RichTextType.Image:
                    var texture = TFGame.MenuAtlas[comp.Data];
                    Draw.Texture(texture, new Vector2(position.X + xOffset, position.Y), Color.White);
                    xOffset += texture.Width;
                    break;
            }
        }
        components.Clear();
    }

    private static void ParseText(string text)
    {
        bool startParsing = false;
        bool startBuilding = false;
        StringBuilder builder = new StringBuilder();
        int startIndex = 0;
        int i;
        for (i = startIndex; i < text.Length; i++)
        {
            char c = text[i];
            
            if (startParsing) 
            {
                if (c == 't' && !startBuilding) 
                {
                    if (text[i + 1] != '=') 
                    {
                        Logger.Error("Syntax Error");
                        continue;
                    }
                    
                    i += 2;
                    startBuilding = true;
                }
                
                if (c == '>') 
                {
                    startBuilding = false;
                    startParsing = false;
                    components.Add(new RichTextComponent(RichTextType.Image, builder.ToString(), startIndex));
                    builder.Clear();
                    startIndex = i + 1;
                }

                if (startBuilding) 
                {
                    builder.Append(text[i]);
                }
                continue;
            }

            if (c == '<') 
            {
                components.Add(new RichTextComponent(RichTextType.Text, text.Substring(startIndex, i - startIndex), startIndex));
                startIndex = i;
                startParsing = true;
                continue;
            }
        }
        if (startIndex != i)
        {
            components.Add(new RichTextComponent(RichTextType.Text, text.Substring(startIndex, i - startIndex), startIndex));
        }
    }
}