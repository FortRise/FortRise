using System;
using System.Collections.Generic;
using System.Xml;
using Monocle;
using Loader = FortRise.Content.IFortRiseContentApi.ILoaderAPI.Loader;

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

