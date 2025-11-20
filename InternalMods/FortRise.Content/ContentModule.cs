using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FortRise.Content;

internal sealed class ContentModule : Mod
{
    public static ContentModule Instance = null!;
    public static ModuleMetadata CurrentModMetadata = null!;


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

        CurrentModMetadata = e.ModContent.Metadata;

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

        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.Atlas, contentConfiguration.Loaders.GetOrNull("atlas"));
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.MenuAtlas, contentConfiguration.Loaders.GetOrNull("menuAtlas"));
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.BGAtlas, contentConfiguration.Loaders.GetOrNull("bgAtlas"));
        SubtextureLoader.Load(registry, content, SubtextureAtlasDestination.BossAtlas, contentConfiguration.Loaders.GetOrNull("bossAtlas"));

        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Main, contentConfiguration.Loaders.GetOrNull("spriteData"));
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Menu, contentConfiguration.Loaders.GetOrNull("menuSpriteData"));
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.BG, contentConfiguration.Loaders.GetOrNull("bgSpriteData"));
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Boss, contentConfiguration.Loaders.GetOrNull("bossSpriteData"));
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Corpse, contentConfiguration.Loaders.GetOrNull("corpseSpriteData"));

        MusicLoader.Load(registry, content, contentConfiguration.Loaders.GetOrNull("music"));
        ArcherLoader.Load(registry, content, contentConfiguration.Loaders.GetOrNull("archerData"));
        TilesetLoader.Load(registry, content, contentConfiguration.Loaders.GetOrNull("tilesetData"));
        ThemeLoader.Load(registry, content, contentConfiguration.Loaders.GetOrNull("themeData"));
        MapRendererLoader.Load(registry, content, contentConfiguration.Loaders.GetOrNull("mapData"));
        VersusLoader.Load(registry, content, Logger);
        QuestLoader.Load(registry, content);
        DarkWorldLoader.Load(registry, content);
        TrialsLoader.Load(registry, content, Logger);
    }

    internal static IReadOnlyDictionary<string, Loader> GetDefaultLoaderConfiguration()
    {
        return new Dictionary<string, Loader>()
        {
            ["atlas"] = new Loader() { Path = ["Content/Atlas/atlas.xml"] },
            ["menuAtlas"] = new Loader() 
            {
                Path = ["Content/Atlas/menuAtlas.xml"]
            },
            ["bgAtlas"] = new Loader() 
            {
                Path = ["Content/Atlas/bgAtlas.xml"]
            },
            ["bossAtlas"] = new Loader() 
            {
                Path = ["Content/Atlas/bossAtlas.xml"]
            },
            ["archerData"] = new Loader() 
            {
                Path = ["Content/Atlas/GameData/archerData.xml"]
            },

            ["spriteData"] = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/spriteData.xml"]
            },

            ["bgSpriteData"] = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/bgSpriteData.xml"]
            },

            ["menuSpriteData"] = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/menuSpriteData.xml"]
            },

            ["bossSpriteData"] = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/bossSpriteData.xml"]
            },

            ["corpseSpriteData"] = new Loader() 
            {
                Path = ["Content/Atlas/SpriteData/corpseSpriteData.xml"]
            },
            ["tilesetData"] = new Loader() 
            {
                Path = ["Content/Atlas/GameData/tilesetData.xml"]
            },
            ["themeData"] = new Loader() 
            {
                Path = ["Content/Atlas/GameData/themeData.xml"]
            },
            ["music"] = new Loader() 
            {
                Path = ["Content/Music/*.ogg", "Content/Music/*.wav"]
            },
            ["mapData"] = new Loader()
            {
                Path = ["Content/Atlas/GameData/mapData.xml"]
            }
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
