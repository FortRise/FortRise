#nullable enable
using System;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise;

internal class SubtextureEntry : ISubtextureEntry
{
    public string ID { get; init; }
    public IResourceInfo? Path { get; init; }
    public Func<Subtexture>? Callback { get; set; }
    public Subtexture? Subtexture => GetActualSubtexture();
    public SubtextureAtlasDestination AtlasDestination { get; init; }

    private Subtexture? cache;

    public SubtextureEntry(string id, IResourceInfo path, SubtextureAtlasDestination destination)
    {
        ID = id;
        Path = path;
        AtlasDestination = destination;
    }

    public SubtextureEntry(string id, Func<Subtexture> callback, SubtextureAtlasDestination destination)
    {
        ID = id;
        Callback = callback;
        AtlasDestination = destination;
    }

    private Subtexture? GetActualSubtexture()
    {
        if (cache != null)
        {
            return cache;
        }
        if (Path != null)
        {
            using var stream = Path.Stream;
            var tex2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, stream);
            return cache = new Subtexture(new Monocle.Texture(tex2D));
        }

        return cache = Callback?.Invoke();
    }
}
