using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    public interface ILoaderAPI
    {
        IContentConfiguration? GetContentConfiguration(ModuleMetadata metadata);

        public interface IContentConfiguration
        {
            IReadOnlyDictionary<string, Loader> Loaders { get; set; }
        }

        public class Loader
        {
            [JsonPropertyName("path")]
            [JsonConverter(typeof(StringOrStringArrayConverter))]
            public string[]? Path { get; set; }

            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; } = true;
        }
    }
}

