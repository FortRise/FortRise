using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Loader = FortRise.Content.IFortRiseContentApi.ILoaderAPI.Loader;

namespace FortRise.Content;

internal sealed partial class ApiImplementation
{
    internal sealed class LoaderAPI : IFortRiseContentApi.ILoaderAPI
    {
        public IFortRiseContentApi.ILoaderAPI.IContentConfiguration? GetContentConfiguration(
            ModuleMetadata metadata)
        {
            if (!ContentModule.Instance.Context.Interop.IsModDepends(metadata))
            {
                return null;
            }

            var mod = ContentModule.Instance.Context.Interop.GetMod(metadata.Name);
            
            if (mod is null)
            {
                return null;
            }

            var content = mod.Content;
            if (content.Root.TryGetRelativePath("content.json", out var contentJson))
            {
                using var stream = contentJson.Stream;
                return JsonSerializer.Deserialize(
                    stream, 
                    ContentConfigurationContext.Default.ContentConfiguration)
                    ?? ContentModule.GetDefaultConfiguration();
            }

            return ContentModule.GetDefaultConfiguration();
        }
    }
}

[JsonSerializable(typeof(ContentConfiguration))]
[JsonConverter(typeof(StringOrStringArrayConverter))]
internal partial class ContentConfigurationContext : JsonSerializerContext {}

internal class ContentConfiguration : IFortRiseContentApi.ILoaderAPI.IContentConfiguration
{
    [JsonPropertyName("loader")]
    public required IReadOnlyDictionary<string, Loader> Loaders { get; set; }
}

internal sealed class StringOrStringArrayConverter : JsonConverter<string[]>
{
    public override string[]? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return [reader.GetString()!];
        }
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<string[]>(ref reader, options);
        }
        
        throw new JsonException(
            $"Unexpected token type: {reader.TokenType}. Expected String or StringArray.");
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
