using System.Text.Json.Serialization;

namespace FortRise;

public struct SFXSoundBank()
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "sfx";

    [JsonPropertyName("count")]
    public int Count { get; set; } = 0;

    [JsonPropertyName("obeysMasterPitch")]
    public bool ObeysMasterPitch { get; set; } = true;

    [JsonPropertyName("instances")]
    public int Instances { get; set; } = 2;

    [JsonPropertyName("variations")]
    public string[] Variations { get; set; } = null;
}