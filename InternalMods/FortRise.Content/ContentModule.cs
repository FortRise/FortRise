using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace FortRise.Content;

internal sealed class ContentModule : Mod
{
    public static ContentModule Instance = null!;


    public ContentModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Events.OnBeforeModInstantiation += OnBeforeModInstantiation;
    }

    public override object GetApi() => new ApiImplementation();

    private void OnBeforeModInstantiation(object? sender, BeforeModInstantiationEventArgs e)
    {
        if (!e.Context.Interop.IsModDepends(ModContent.Metadata))
        {
            return;
        }

        var content = e.ModContent;
        var registry = e.Context.Registry;
        if (registry is null)
        {
            return;
        }

        ContentConfiguration contentConfiguration;
        if (content.Root.TryGetRelativePath("content.json", out var contentJson))
        {
            using var stream = contentJson.Stream;
            contentConfiguration = JsonSerializer.Deserialize(stream, ContentConfigurationContext.Default.ContentConfiguration) 
                ?? GetDefaultConfiguration();
        }
        else 
        {
            contentConfiguration = GetDefaultConfiguration();
        }

        contentConfiguration.Loaders ??= GetDefaultLoaderConfiguration();

        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.Atlas, contentConfiguration.Loaders.Atlas);
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.MenuAtlas, contentConfiguration.Loaders.MenuAtlas);
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.BGAtlas, contentConfiguration.Loaders.BGAtlas);
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.BossAtlas, contentConfiguration.Loaders.BossAtlas);

        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Main, contentConfiguration.Loaders.SpriteData);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Menu, contentConfiguration.Loaders.MenuSpriteData);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.BG, contentConfiguration.Loaders.BgSpriteData);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Boss, contentConfiguration.Loaders.BossSpriteData);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Corpse, contentConfiguration.Loaders.CorpseSpriteData);

        MusicLoader.Load(registry, content);
        ArcherLoader.Load(registry, content, contentConfiguration.Loaders.ArcherData);
        TilesetLoader.Load(registry, content);
        ThemeLoader.Load(registry, content);
        VersusLoader.Load(registry, content, Logger);
        QuestLoader.Load(registry, content);
        DarkWorldLoader.Load(registry, content);
        TrialsLoader.Load(registry, content, Logger);
    }

    internal static LoaderConfiguration GetDefaultLoaderConfiguration()
    {
        return new()
        {
            Atlas = new Loader() 
            {
                Path = ["Content/Atlas/atlas.xml"]
            },
            MenuAtlas = new Loader() 
            {
                Path = ["Content/Atlas/menuAtlas.xml"]
            },
            BGAtlas = new Loader() 
            {
                Path = ["Content/Atlas/bgAtlas.xml"]
            },
            BossAtlas = new Loader() 
            {
                Path = ["Content/Atlas/bossAtlas.xml"]
            },
            ArcherData = new Loader() 
            {
                Path = ["Content/Atlas/GameData/archerData.xml"]
            },

            SpriteData = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/spriteData.xml"]
            },

            BgSpriteData = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/bgSpriteData.xml"]
            },

            MenuSpriteData = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/menuSpriteData.xml"]
            },

            BossSpriteData = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/bossSpriteData.xml"]
            },

            CorpseSpriteData = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/corpseSpriteData.xml"]
            },
        };
    }

    internal static ContentConfiguration GetDefaultConfiguration()
    {
        var config = new ContentConfiguration() 
        {
            Loaders = GetDefaultLoaderConfiguration()
        };

        return config;
    }
}
