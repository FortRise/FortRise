using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortRise;

public class ModuleMetadata : IEquatable<ModuleMetadata>
{
    public string Name { get; set; }
    public Version Version { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DLL { get; set; } = string.Empty;
    public ModuleMetadata[] Dependencies { get; set; } = null;
    public string NativePath { get; set; } = string.Empty;
    public string NativePathX86 { get; set; } = string.Empty;

    public string PathDirectory = string.Empty;
    public string PathZip = string.Empty;

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
        var version = Version.Major.GetHashCode();
        var name = Name.GetHashCode();
        return version + name;
    }

    public static ModuleMetadata ParseMetadata(string dir, string path)
    {
        using var jfs = File.OpenRead(path);
        return ParseMetadata(dir, jfs);
    }

    private static JsonSerializerOptions metaJsonOptions = new JsonSerializerOptions 
    {
        PropertyNameCaseInsensitive = true
    };

    public static ModuleMetadata ParseMetadata(string dirPath, Stream stream, bool zip = false)
    {
        var metadata = JsonSerializer.Deserialize<ModuleMetadata>(stream, metaJsonOptions);
        var fortRise = metadata.GetFortRiseMetadata();
        if (fortRise != null)
        {
            if (RiseCore.FortRiseVersion < fortRise.Version)
            {
                Logger.Error($"Mod Name: {metadata.Name} has a higher version of FortRise required {fortRise.Version}. Your FortRise version: {RiseCore.FortRiseVersion}");
                return null;
            }
        }
        else 
        {
            Logger.Error("FortRise dependency cannot be found, this will be invalid later version of FortRise");
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

    public ModuleMetadata GetFortRiseMetadata() 
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
}
