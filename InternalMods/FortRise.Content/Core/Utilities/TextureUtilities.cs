using System;
using System.Xml;
using TowerFall;

namespace FortRise.Content;

internal static class TextureUtilities 
{
    public static ISubtextureEntry LoadTexture(this IModContent content, IModRegistry registry, XmlElement? xmlOrText, SubtextureAtlasDestination atlas)
    {
        switch (xmlOrText!.FirstChild)
        {
            case XmlElement elm:
                var sub = SubtextureLoader.LoadSubtexture(content, registry, elm, atlas, null) 
                    ?? throw new InvalidOperationException("Texture xml is invalid or file does not exists.");
                return sub;
            case XmlText text: 
                return LoadTexture(content, registry, text.InnerText.Trim(), atlas); // legacy
            default:
                throw new InvalidOperationException("Texture xml is invalid.");
        };
    }

    public static ISubtextureEntry LoadTexture(this IModContent content, IModRegistry registry, string path, SubtextureAtlasDestination atlas)
    {
        if (content.Root.TryGetRelativePath(path, out var info))
        {
            return registry.Subtextures.RegisterTexture(info, atlas);
        }
        else 
        {
            var texture = registry.Subtextures.GetTextureWithRelative(path, atlas);

            if (texture is not null)
            {
                return texture;
            }

            var ids = VanillaXmlCacher.GetAllAvailableSubtexturesID();

            if (!ids.Contains(path))
            {
                throw new TextureNotFoundException($"'{path}' does not exists on this mod or in the game. Cannot provide a fallback.");
            }

            return atlas switch 
            {
                SubtextureAtlasDestination.Atlas => registry.Subtextures.RegisterTexture(() => TFGame.Atlas[path], atlas),
                SubtextureAtlasDestination.BGAtlas => registry.Subtextures.RegisterTexture(() => TFGame.BGAtlas[path], atlas),
                SubtextureAtlasDestination.MenuAtlas => registry.Subtextures.RegisterTexture(() => TFGame.MenuAtlas[path], atlas), 
                SubtextureAtlasDestination.BossAtlas => registry.Subtextures.RegisterTexture(() => TFGame.BossAtlas[path], atlas),
                _ => throw new NotImplementedException()
            };
        }
    }
}

internal sealed class TextureNotFoundException : Exception
{
    public TextureNotFoundException(string? message) : base(message)
    {
    }
}
