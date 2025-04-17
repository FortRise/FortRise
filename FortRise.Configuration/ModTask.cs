using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortRise.Configuration;

public sealed class ModTask : Task
{
    [Required]
    public string ModName { get; set; } = null!;

    [Required]
    public string ModVersion { get; set; } = null!;

    [Required]
    public string ModPublishPath { get; set; } = null!;

    [Required]
    public string ModZipPath { get; set; } = null!;

    [Required]
    public string ModProjectDir { get; set; } = null!;

    [Required]
    public string ModTargetDir { get; set; } = null!;

    [Required]
    public bool ModEnableZip { get; set; }

    [Required]
    public bool ModEnablePublish { get; set; }

    public override bool Execute()
    {
        if (!ModEnablePublish && !ModEnableZip)
        {
            return true;
        }

        var files = GetAllFiles(ModProjectDir, ModTargetDir);

        if (ModEnablePublish)
        {
            DeployMod(files, Path.Combine(ModPublishPath, ModName));
        }

        if (ModEnableZip)
        {
            ZipMod(files, ModZipPath);
        }

        return true;
    }

    private List<ReadFile> GetAllFiles(string projectDir, string targetDir)
    {
        var list = new List<ReadFile>();
        var projectDirUri = new Uri(projectDir);
        var targetDirUri = new Uri(targetDir);

        var metadataFile = Path.Combine(projectDir, "meta.json");

        if (File.Exists(metadataFile))
        {
            list.Add(new ReadFile(Path.GetFullPath(metadataFile), projectDirUri.MakeRelativeUri(new Uri(Path.GetFullPath(metadataFile))).OriginalString));
        }

        var fortIgnore = Path.Combine(projectDir, ".fortriseignore");

        var ignore = new Ignore.Ignore();

        if (File.Exists(fortIgnore))
        {
            using (var tr = File.OpenText(fortIgnore))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    ignore.Add(line);
                }
            }
        }


        foreach (var file in Directory.EnumerateFiles(targetDir, "*", SearchOption.AllDirectories))
        {
            if (file.Equals(".DS_Store", StringComparison.InvariantCultureIgnoreCase) ||
                file.Equals("Thumbs.db", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            if (file.EndsWith(".deps.json"))
            {
                continue;
            }

            if (ignore.IsIgnored(file))
            {
                continue;
            }

            list.Add(new ReadFile(file, targetDirUri.MakeRelativeUri(new Uri(file)).OriginalString));
        }

        return list;
    }

    private void DeployMod(List<ReadFile> files, string destination)
    {
        foreach (var file in files)
        {
            var from = file;
            var to = Path.Combine(destination, file.Relative);

            Directory.CreateDirectory(Path.GetDirectoryName(to));

            if (!TryMetadataRewrite(file, out string? meta))
            {
                throw new Exception("Cannot proceed as 'meta.json' not found!");
            }

            if (meta != null)
            {
                File.WriteAllText(to, meta);
            }
            else 
            {
                File.Copy(from.Absolute, to, true);
            }
        }
    }

    private void ZipMod(List<ReadFile> files, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        using var zipStream = new FileStream(destination, FileMode.Create, FileAccess.Write);
		using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);


		foreach (var file in files)
		{
			var from = file;
			var zipEntryName = file.Relative.Replace(Path.DirectorySeparatorChar, '/');

            if (!TryMetadataRewrite(file, out string? meta))
            {
                throw new Exception("Cannot proceed as 'meta.json' not found!");
            }

			using var fileStreamInZip = archive.CreateEntry(zipEntryName).Open();

            if (meta != null)
            {
                using var tw = new StreamWriter(fileStreamInZip);
                tw.Write(meta);
            }
            else 
            {
                using var fs = File.OpenRead(file.Absolute);
                fs.CopyTo(fileStreamInZip);
            }
		}
    }

    private bool TryMetadataRewrite(ReadFile readFile, out string? metadataJson)
    {
        JsonSerializerOptions metaJsonOptions = new JsonSerializerOptions 
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        if (readFile.Relative != "meta.json")
        {
            metadataJson = null;
            return true;
        }

        string json;
        using (var fs = File.OpenText(readFile.Absolute)) 
        {
            json = fs.ReadToEnd();
        }

        var meta = JsonSerializer.Deserialize<ModuleMetadata>(json, metaJsonOptions)!;

        if (string.IsNullOrEmpty(meta.Name))
        {
            meta.Name = ModName;
        }

        string? version = meta.Version;

        if (string.IsNullOrEmpty(version))
        {
            Log.LogError($"The '{readFile.Relative}' file does missing a required 'Version' field.");
            metadataJson = null;
            return false;
        }

        if (version!.Trim() != ModVersion.Trim())
        {
            Log.LogError($"The '{readFile.Relative}' file specifies a version of \"{version.Trim()}\" which does not match the <ModVersion> property of {ModVersion.Trim()}");
            metadataJson = null;
            return false;
        }


        metadataJson = JsonSerializer.Serialize(meta, metaJsonOptions);
        return true;
    }
}

public record struct ReadFile(string Absolute, string Relative);

public class ModDependency 
{
    public string? Name { get; set; }
    public string? Version { get; set; }
}

public class ModuleMetadata 
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string DLL { get; set; } = string.Empty;
    public string[]? Tags { get; set; }
    public ModDependency[] Dependencies { get; set; } = null!;
    public ModDependency[] OptionalDependencies { get; set; } = null!;
    public ModuleUpdater Update { get; set; } = null!;
}

public class ModuleUpdater 
{
    [JsonPropertyName("Github")]
    public Github? GH { get; set; }

    public class Github 
    {
        public string? Repository { get; set; }
        public string? TagRegex { get; set; }
    }
}