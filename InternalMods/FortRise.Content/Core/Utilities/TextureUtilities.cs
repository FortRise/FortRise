using System;
using TowerFall;

namespace FortRise.Content;

internal static class TextureUtilities 
{
    public static ISubtextureEntry LoadTexture(this IModContent content, IModRegistry registry, string path, SubtextureAtlasDestination atlas)
    {
        if (content.Root.TryGetRelativePath(path, out var info))
        {
            return registry.Subtextures.RegisterTexture(info, atlas);
        }
        else 
        {
            var ids = VanillaXmlCacher.GetAllAvailableSubtexturesID();

            if (!ids.Contains(path))
            {
                throw new Exception($"'{path}' does not exists on this mod or in the game. Cannot provide a fallback.");
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
