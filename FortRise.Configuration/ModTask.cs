using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using System.IO.Compression;
using System.Text.Json;
using MAB.DotIgnore;
using System.Linq;

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

        IgnoreList? ignore = null;

        if (File.Exists(fortIgnore))
        {
            ignore = new IgnoreList(fortIgnore);
        }

        AddToList(new DirectoryInfo(targetDir), ignore);

        void AddToList(DirectoryInfo source, IgnoreList? ignore)
        {
            bool IsNotIgnoredDirectory(DirectoryInfo x)
            {
                if (ignore is null)
                {
                    return true;
                }
                return !ignore.IsIgnored(x);
            }

            bool IsNotIgnoredFile(FileInfo x)
            {
                if (ignore is null)
                {
                    return true;
                }
                return !ignore.IsIgnored(x);
            }

            foreach (var info in source.GetDirectories().Where(IsNotIgnoredDirectory))
            {
                AddToList(info, ignore);
            }

            foreach (var file in source.GetFiles().Where(IsNotIgnoredFile))
            {
                if (file.Name.Equals(".DS_Store", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (file.Name.Equals("Thumbs.db", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (file.Name.EndsWith(".deps.json"))
                {
                    continue;
                }

                list.Add(new ReadFile(file.FullName, targetDirUri.MakeRelativeUri(new Uri(file.FullName)).OriginalString));
            }
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
            WriteIndented = true,
        };

        metadataJson = null;

        if (readFile.Relative != "meta.json")
        {
            return true;
        }


        string json;
        using (var fs = File.OpenText(readFile.Absolute)) 
        {
            json = fs.ReadToEnd();
        }

        var meta = JsonSerializer.Deserialize<InsensitiveDictionary>(json, metaJsonOptions)!;

        if (!meta.ContainsKey("Name"))
        {
            meta["Name"] = ModName;
        }

        if (!meta.TryGetValue("Version", out object? obj))
        {
            Log.LogError($"The '{readFile.Relative}' file does missing a required 'Version' field.");
            return false;
        }

        string? version = ((JsonElement)obj).GetString();


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

internal sealed class InsensitiveDictionary() : Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {}