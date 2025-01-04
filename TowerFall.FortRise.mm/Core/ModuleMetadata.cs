using System;
using System.IO;
using TeuJson;

namespace FortRise;

public class ModuleMetadata : IEquatable<ModuleMetadata>, IDeserialize
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

    public static ModuleMetadata ParseMetadata(string dirPath, Stream path, bool zip = false)
    {
        var json = JsonTextReader.FromStream(path);
        var metadata = json.Convert<ModuleMetadata>();
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

    public void Deserialize(JsonObject value)
    {
        Name = value["name"];
        Version = new Version(value.GetJsonValueOrNull("version") ?? "1.0.0");
        var fVersion = value.GetJsonValueOrNull("required");
        if (fVersion == null)
            FortRiseVersion = RiseCore.FortRiseVersion;
        else
            FortRiseVersion = new Version(fVersion);
        Description = value.GetJsonValueOrNull("description") ?? string.Empty;
        Author = value.GetJsonValueOrNull("author") ?? string.Empty;
        DLL = value.GetJsonValueOrNull("dll") ?? string.Empty;
        NativePath = value.GetJsonValueOrNull("nativePath") ?? string.Empty;
        NativePathX86 = value.GetJsonValueOrNull("nativePathX86") ?? string.Empty;
        var dep = value.GetJsonValueOrNull("dependencies");
        if (dep is null)
            return;
        Dependencies = dep.ConvertToArray<ModuleMetadata>();
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
