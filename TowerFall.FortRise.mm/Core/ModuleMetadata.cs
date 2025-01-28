using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FortRise;

public class ModuleMetadata : IEquatable<ModuleMetadata>
{
    public string Name;
    public Version Version;
    public Version FortRiseVersion;
    public string Description;
    public string Author;
    public string DLL;
    public string PathDirectory = string.Empty;
    public string PathZip = string.Empty;
    public ModuleMetadata[] Dependencies;
    public string NativePath;
    public string NativePathX86;

    public bool IsZipped => !string.IsNullOrEmpty(PathZip);
    public bool IsDirectory => !string.IsNullOrEmpty(PathDirectory);

    public ModuleMetadata() {}


    public override string ToString()
    {
        return $"Metadata: {Name} by {Author} {Version}";
    }


    public bool Equals(ModuleMetadata other)
    {
        if (other.Name != this.Name)
            return false;

        if (other.Version.Major != this.Version.Major)
            return false;

        if (this.Version.Minor < other.Version.Minor)
            return false;

        return true;
    }

    public override bool Equals(object obj) => Equals(obj as ModuleMetadata);


    public override int GetHashCode()
    {
        var version = Version.Major.GetHashCode() + Version.Minor.GetHashCode();
        var name = Name.GetHashCode();
        return version + name;
    }

    public static ModuleMetadata ParseMetadata(string dir, string path)
    {
        using var jfs = File.OpenRead(path);
        return ParseMetadata(dir, jfs);
    }

    public static ModuleMetadata ParseMetadata(string dirPath, Stream stream, bool zip = false)
    {
        var metadataJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream);
        var metadata = CreateModuleMetadataFromJson(metadataJson);
        if (RiseCore.FortRiseVersion < metadata.FortRiseVersion)
        {
            Logger.Error($"Mod Name: {metadata.Name} has a higher version of FortRise required {metadata.FortRiseVersion}. Your FortRise version: {RiseCore.FortRiseVersion}");
            return null;
        }
        string zipPath = "";
        if (!zip)
        {
            metadata.PathDirectory = dirPath;
        }
        else
        {
            zipPath = dirPath;
            dirPath = Path.GetDirectoryName(dirPath);
            metadata.PathZip = zipPath;
        }

        return metadata;
    }

    public static ModuleMetadata CreateModuleMetadataFromJson(Dictionary<string, JsonElement> value)
    {
        var meta = new ModuleMetadata();
        meta.Name = value["name"].GetString();
        meta.Version = new Version(value.GetStringOrNull("version") ?? "1.0.0");
        var fVersion = value.GetStringOrNull("required");
        if (fVersion == null)
            meta.FortRiseVersion = RiseCore.FortRiseVersion;
        else
            meta.FortRiseVersion = new Version(fVersion);
        meta.Description = value.GetStringOrNull("description") ?? string.Empty;
        meta.Author = value.GetStringOrNull("author") ?? string.Empty;
        meta.DLL = value.GetStringOrNull("dll") ?? string.Empty;
        meta.NativePath = value.GetStringOrNull("nativePath") ?? string.Empty;
        meta.NativePathX86 = value.GetStringOrNull("nativePathX86") ?? string.Empty;

        // TODO Improve the dependency system
        // if (value.TryGetValue("dependencies", out var deps))
        // {
        //     var list = new List<ModuleMetadata>();
        //     foreach (var dep in deps.EnumerateArray())
        //     {
        //     }

        //     meta.Dependencies = list.ToArray();
        // }

        return meta;
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
}
