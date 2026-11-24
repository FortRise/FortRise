using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortRise.ContentPatcher;

internal sealed class PatchContent
{
    [JsonPropertyName("includes")]
    public List<string> Includes { get; set; } = [];

    [JsonPropertyName("patches")]
    public List<PatchDefinition> Patches { get; set; } = [];
}

internal sealed class PatchDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("atlas")]
    public SubtextureAtlasDestination Atlas { get; set; } = SubtextureAtlasDestination.Atlas;

    [JsonPropertyName("replace")]
    public string? Replace { get; set; }

    [JsonPropertyName("where")]
    public Dictionary<string, JsonElement>? Where { get; set; }

    [JsonPropertyName("changes")]
    public List<JsonElement>? Changes { get; set; }
}
