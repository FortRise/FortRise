using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FortRise.ContentPatcher;

internal sealed class PatchCondition
{
    private List<string>? themes;
    private List<string>? filenames;
    private List<string>? modes;
    private bool platformMatches = true;

    public bool RequiresLevel => themes is not null || filenames is not null || modes is not null;

    public bool Matches(IFortRiseContentPatcherAPI.ILevelInfo? level)
    {
        if (!platformMatches)
        {
            return false;
        }

        if (!RequiresLevel)
        {
            return true;
        }

        if (level is null)
        {
            return false;
        }

        if (themes is not null && !themes.Exists(level.IsTower))
        {
            return false;
        }

        if (filenames is not null && !filenames.Exists(f => string.Equals(f, level.Filename, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (modes is not null && !modes.Exists(m =>
            string.Equals(m, level.Mode.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(m, level.ModeName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    public static bool TryParse(ILogger logger, string mod, string id, Dictionary<string, JsonElement>? where, out PatchCondition? condition)
    {
        condition = null;
        if (where is not { Count: > 0 })
        {
            return true;
        }

        condition = new PatchCondition();
        foreach (var (key, value) in where)
        {
            if (!TryReadStrings(value, out var values))
            {
                logger.LogError("[{mod}] '{id}': 'where.{key}' must be a string or an array of strings.", mod, id, key);
                return false;
            }

            switch (key)
            {
                case "theme":
                    condition.themes = values;
                    break;
                case "filename":
                    condition.filenames = values;
                    break;
                case "mode":
                    condition.modes = values;
                    break;
                case "platform":
                    condition.platformMatches = values.Exists(p => IsCurrentPlatform(logger, mod, id, p));
                    break;
                default:
                    logger.LogWarning("[{mod}] '{id}': Unknown 'where' condition '{key}' will be ignored.", mod, id, key);
                    break;
            }
        }
        return true;
    }

    private static bool IsCurrentPlatform(ILogger logger, string mod, string id, string platform)
    {
        switch (platform.ToLowerInvariant())
        {
            case "windows":
                return OperatingSystem.IsWindows();
            case "linux":
                return OperatingSystem.IsLinux();
            case "macos":
            case "osx":
                return OperatingSystem.IsMacOS();
            default:
                logger.LogWarning("[{mod}] '{id}': Unknown platform '{platform}'. Expected 'windows', 'linux' or 'macos'.", mod, id, platform);
                return false;
        }
    }

    private static bool TryReadStrings(JsonElement element, out List<string> values)
    {
        values = [];
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                values.Add(element.GetString()!);
                return true;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        return false;
                    }
                    values.Add(item.GetString()!);
                }
                return true;
            default:
                return false;
        }
    }
}
