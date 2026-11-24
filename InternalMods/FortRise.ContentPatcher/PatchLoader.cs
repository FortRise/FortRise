using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace FortRise.ContentPatcher;

internal sealed class PatchLoader(ILogger logger)
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<PatchDefinition> Load(IModContent content, string entryPath)
    {
        var patches = new List<PatchDefinition>();
        LoadRecursive(content, entryPath, patches, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        return patches;
    }

    private void LoadRecursive(IModContent content, string path, List<PatchDefinition> patches, HashSet<string> visited)
    {
        if (!visited.Add(path))
        {
            logger.LogWarning("[{mod}] Circular include detected: '{path}'", content.Metadata.Name, path);
            return;
        }

        if (!content.Root.TryGetRelativePath(path, out var res))
        {
            logger.LogError("[{mod}] Patch file not found: '{path}'", content.Metadata.Name, path);
            return;
        }

        PatchContent? patchContent;
        try
        {
            using var stream = res.Stream;
            patchContent = JsonSerializer.Deserialize<PatchContent>(stream, options);
        }
        catch (JsonException ex)
        {
            logger.LogError("[{mod}] Failed to parse '{path}': {ex}", content.Metadata.Name, path, ex.Message);
            return;
        }

        if (patchContent is null)
        {
            return;
        }

        foreach (var include in patchContent.Includes)
        {
            LoadRecursive(content, include, patches, visited);
        }

        patches.AddRange(patchContent.Patches);
    }
}
