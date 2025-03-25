using System.Text.Json.Serialization;

namespace FortLauncher;

public record struct LaunchOverride(
    string GamePath
);


[JsonSerializable(typeof(LaunchOverride))]
internal partial class LaunchOverrideContext : JsonSerializerContext {}