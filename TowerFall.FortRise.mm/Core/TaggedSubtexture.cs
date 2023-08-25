using Monocle;

namespace FortRise;

public class TaggedSubtexture : Subtexture
{
    public string[] Tags;
    public TaggedSubtexture(Texture texture, int x, int y, int width, int height, string[] tags) : base(texture, x, y, width, height)
    {
        Tags = tags;
    }
}