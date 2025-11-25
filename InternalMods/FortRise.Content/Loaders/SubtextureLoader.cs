using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise.Content;

internal static class SubtextureLoader 
{
    internal static void Load(IModRegistry registry, IModContent content, SubtextureAtlasDestination destination, Loader? loader)
    {
        loader ??= destination switch
        {
            SubtextureAtlasDestination.Atlas => new Loader()
            {
                Path = ["Content/Atlas/atlas.xml"]
            },
            SubtextureAtlasDestination.MenuAtlas => new Loader()
            {
                Path = ["Content/Atlas/menuAtlas.xml"]
            },
            SubtextureAtlasDestination.BGAtlas => new Loader()
            {
                Path = ["Content/Atlas/bgAtlas.xml"]
            },
            SubtextureAtlasDestination.BossAtlas => new Loader()
            {
                Path = ["Content/Atlas/bossAtlas.xml"]
            },
            _ => throw new NotImplementedException()
        };

        if (loader.Path is null || !loader.Enabled)
        {
            return;
        }

        List<IResourceInfo> resources = [];
        
        foreach (var path in loader.Path)
        {
            resources.AddRange(content.Root.EnumerateChildrens(path));
        }

        foreach (var res in resources)
        {
            LoadAll(content, registry, destination, res);
        }
    }

    private static readonly Dictionary<string, Monocle.Texture> textureCache = [];

    private static void LoadAll(IModContent content, IModRegistry registry, SubtextureAtlasDestination dest, IResourceInfo res)
    {
        var xml = res.Xml ?? 
            throw new Exception($"[{content.Metadata.Name}] Failed to load Xml file {res.Path}.");

        var textureAtlas = xml["TextureAtlas"] ??
            throw new Exception($"[{content.Metadata.Name}] Missing TextureAtlas element.");

        foreach (XmlElement subtexture in textureAtlas.GetElementsByTagName("SubTexture"))
        {
            string name = subtexture.Attr("name");
            string? path = subtexture.Attr("path", null);

            if (path is null)
            {
                if (content.Root.TryGetRelativePath(Path.ChangeExtension(res.Path, "png"), out var r))
                {
                    int x = subtexture.AttrInt("x");
                    int y = subtexture.AttrInt("y");
                    int width = subtexture.AttrInt("width");
                    int height = subtexture.AttrInt("height");

                    registry.Subtextures.RegisterTexture(name, () =>
                    {
                        ref var texture = ref CollectionsMarshal.GetValueRefOrAddDefault(textureCache, res.RootPath, out bool exists);

                        if (!exists)
                        {
                            using var texRes = r.Stream;
                            var tex = Texture2D.FromStream(Engine.Instance.GraphicsDevice, texRes);
                            texture = new Monocle.Texture(tex);
                        }

                        return new Subtexture(texture, new Rectangle(x, y, width, height));
                    }, dest);
                }

                continue;
            }

            registry.Subtextures.RegisterTexture(
                name, 
                content.Root.GetRelativePath(path),
                dest
            );
        }
    }
}
