using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FortRise;

public static class SpriteFontExt
{
    extension(SpriteFont sprite)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "kerning")]
		internal static extern ref List<Vector3> kerning(SpriteFont self);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "croppingData")]
		internal static extern ref List<Rectangle> croppingData(SpriteFont self);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "characterIndexMap")]
		internal static extern ref Dictionary<char, int> characterIndexMap(SpriteFont self);

		public Vector2 MeasureString(ReadOnlySpan<char> text)
		{
			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			Vector2 result = Vector2.Zero;
			float curLineWidth = 0.0f;
			float finalLineHeight = sprite.LineSpacing;
			bool firstInLine = true;

			foreach (char c in text)
			{
				// Special characters
				if (c == '\r')
				{
					continue;
				}
				if (c == '\n')
				{
					result.X = Math.Max(result.X, curLineWidth);
					result.Y += sprite.LineSpacing;
					curLineWidth = 0.0f;
					finalLineHeight = sprite.LineSpacing;
					firstInLine = true;
					continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */


				int index;

                var indexMap = characterIndexMap(sprite);
                
				if (!indexMap.TryGetValue(c, out index))
				{
					if (!sprite.DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = indexMap[sprite.DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				Vector3 cKern = kerning(sprite)[index];
				if (firstInLine)
				{
					curLineWidth += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += sprite.Spacing + cKern.X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += cKern.Y + cKern.Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				int cCropHeight = croppingData(sprite)[index].Height;
				if (cCropHeight > finalLineHeight)
				{
					finalLineHeight = cCropHeight;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}



        public string[] WrapText(string text, float maxWidth)
        {
            return sprite.WrapText(text.AsSpan(), maxWidth);
        }

        public string[] WrapText(ReadOnlySpan<char> text, float maxWidth)
        {
            var result = new List<string>();
            Span<char> t = text.Length <= 256
                ? stackalloc char[text.Length]
                : new char[text.Length];

            text.ToUpperInvariant(t);

            foreach (var range in text.Split('\n'))
            {
                var paragraph = text[range.Start..range.End];
                ReadOnlySpan<char> current = "";
                foreach (var paragraphRange in paragraph.Split((char)32))
                {
                    var word = paragraph[paragraphRange.Start..paragraphRange.End];
                    if (word == " ")
                    {
                        continue; // skip empty entries
                    }

                    ReadOnlySpan<char> wordCandidate = current.Length == 0 
                        ? word
                        : $"{current} {word}";

                    if (current.Length > 0 && sprite.MeasureString(wordCandidate).X > maxWidth)
                    {
                        result.Add(current.ToString());
                        current = word;
                    }
                    else
                    {
                        current = wordCandidate;
                    }
                }

                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                }
            }

            return [.. result];
        }
    }
}
