#nullable enable
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FortRise;

public class ModuleUpdater 
{
    [JsonPropertyName("Github")]
    public ModPublisher.Github? GH { get; set; }
    [JsonPropertyName("GameBanana")]
    public ModPublisher.GameBanana? GB { get; set; }

    public abstract record ModPublisher 
    {
        public record Github(string? Repository, string? TagRegex) : ModPublisher;
        public record GameBanana(int? ID) : ModPublisher;
    }
}


public partial class ModuleMetadata : IEquatable<ModuleMetadata>
{
    public string Name { get; set; } = null!;
    public SemanticVersion Version { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DLL { get; set; } = string.Empty;
    public string JS { get; set; } = string.Empty;
    public string[]? Tags { get; set; } = null;
    public ModuleMetadata[]? Dependencies { get; set; } = null;
    public ModuleMetadata[]? OptionalDependencies { get; set; } = null;
    public ModuleUpdater? Update { get; set; } = null;

    public string PathDirectory = string.Empty;
    public string PathZip = string.Empty;
    [JsonIgnore]
    internal ModAssemblyLoadContext? AssemblyLoadContext { get; set; } = null;

    [JsonIgnore]
    internal byte[] Hash => RiseCore.GetChecksum(this);

    public bool IsZipped => !string.IsNullOrEmpty(PathZip);
    public bool IsDirectory => !string.IsNullOrEmpty(PathDirectory);

    public ModuleMetadata() {}


    public override string ToString()
    {
        return $"Metadata: {Name} by {Author} {Version}";
    }


    public bool Equals(ModuleMetadata? other)
    {
        if (other is null)
            return false;

        if (other.Name != this.Name)
            return false;

        if (other.Version.Major != this.Version.Major)
            return false;

        if (this.Version.Minor < other.Version.Minor)
            return false;

        if (this.Version.Prerelease != other.Version.Prerelease)
            return false;
        

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ModuleMetadata);


    public override int GetHashCode()
    {
        var version = Version.Major.GetHashCode();
        var name = Name.GetHashCode();
        return version + name;
    }

    private static JsonSerializerOptions metaJsonOptions;

    static ModuleMetadata()
    {
        metaJsonOptions = new JsonSerializerOptions 
        {
            PropertyNameCaseInsensitive = true,
        };
        metaJsonOptions.Converters.Add(new SemanticVersionConverter());
    }

    public static Result<ModuleMetadata, string> ParseMetadata(string dir, string path)
    {
        using var jfs = File.OpenRead(path);
        return ParseMetadata(dir, jfs);
    }

    public static Result<ModuleMetadata, string> ParseMetadata(string dirPath, Stream stream, bool zip = false)
    {
        var metadata = JsonSerializer.Deserialize<ModuleMetadata>(stream, metaJsonOptions);
        if (metadata is null)
        {
            return Result<ModuleMetadata, string>.Error($"Json failed to parse on directory: '{dirPath}'");
        }
        var regex = GeneratedNameRegex();

        if (!regex.IsMatch(metadata.Name))
        {
            return Result<ModuleMetadata, string>.Error($"Mod Name: {metadata.Name} name pattern is not allowed. Pattern allowed: (AaZz09_)");
        }

        var fortRise = metadata.GetFortRiseMetadata();
        if (fortRise! != null!)
        {
            if (RiseCore.FortRiseVersion < fortRise.Version)
            {
                return Result<ModuleMetadata, string>.Error($"Mod Name: {metadata.Name} has a higher version of FortRise required {fortRise.Version}. Your FortRise version: {RiseCore.FortRiseVersion}");
            }
        }
        else 
        {
            return Result<ModuleMetadata, string>.Error($"Mod Name: {metadata.Name} does not have FortRise dependency.");
        }

        string zipPath;
        if (!zip)
        {
            metadata.PathDirectory = dirPath;
        }
        else
        {
            zipPath = dirPath;
            metadata.PathZip = zipPath;
        }

        return metadata;
    }

    public static bool operator ==(ModuleMetadata lhs, ModuleMetadata rhs)
    {
        if (rhs is null)
        {
            if (lhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ModuleMetadata lhs, ModuleMetadata rhs) => !(lhs == rhs);

    public ModuleMetadata? GetFortRiseMetadata() 
    {
        if (Dependencies == null)
        {
            return null;
        }
        foreach (var dep in Dependencies)
        {
            if (dep.Name == "FortRise")
            {
                return dep;
            }
        }
        return null;
    }

    [GeneratedRegex(@"^[\w\\s.]+$")]
    private static partial Regex GeneratedNameRegex();
}
