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

        SubtextureLoader.Load(registry, content);

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

    private static LoaderConfiguration GetDefaultLoaderConfiguration()
    {
        return new()
        {
            ArcherData = new() 
            {
                Path = ["Content/Atlas/GameData/archerData.xml"]
            },

            SpriteData = new() 
            {
                Path = ["Content/Atlas/SpriteData/spriteData.xml"]
            },

            BgSpriteData = new() 
            {
                Path = ["Content/Atlas/SpriteData/bgSpriteData.xml"]
            },

            MenuSpriteData = new() 
            {
                Path = ["Content/Atlas/SpriteData/menuSpriteData.xml"]
            },

            BossSpriteData = new() 
            {
                Path = ["Content/Atlas/SpriteData/bossSpriteData.xml"]
            },

            CorpseSpriteData = new() 
            {
                Path = ["Content/Atlas/SpriteData/corpseSpriteData.xml"]
            },
        };
    }

    private static ContentConfiguration GetDefaultConfiguration()
    {
        var config = new ContentConfiguration() 
        {
            Loaders = GetDefaultLoaderConfiguration()
        };

        return config;
    }
}

[JsonSerializable(typeof(ContentConfiguration))]
[JsonConverter(typeof(StringOrStringArrayConverter))]
internal partial class ContentConfigurationContext : JsonSerializerContext {}

internal class ContentConfiguration 
{
    [JsonPropertyName("loader")]
    public LoaderConfiguration? Loaders { get; set; } 
}

internal class LoaderConfiguration 
{
    [JsonPropertyName("archerData")]
    public Loader? ArcherData { get; set; }

    [JsonPropertyName("spriteData")]
    public Loader? SpriteData { get; set; }

    [JsonPropertyName("menuSpriteData")]
    public Loader? MenuSpriteData { get; set; }

    [JsonPropertyName("bgSpriteData")]
    public Loader? BgSpriteData { get; set; }

    [JsonPropertyName("bossSpriteData")]
    public Loader? BossSpriteData { get; set; }

    [JsonPropertyName("corpseSpriteData")]
    public Loader? CorpseSpriteData { get; set; }
}

internal class Loader 
{
    public Loader()
    {
    }

    [JsonPropertyName("path")]
    [JsonConverter(typeof(StringOrStringArrayConverter))]
    public string[]? Path { get; set; }
}

internal sealed class StringOrStringArrayConverter : JsonConverter<string[]>
{
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return [reader.GetString()!];
        }
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<string[]>(ref reader, options);
        }
        
        throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected String or StringArray.");
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
