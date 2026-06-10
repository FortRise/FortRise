#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TowerFall;

namespace FortRise;

public static class ArrowsRegistry
{
    private static Dictionary<string, IArrowEntry> arrowEntries = [];
    private static Dictionary<ArrowTypes, IArrowEntry> typesToArrowEntries = [];
    public static Dictionary<string, ArrowTypes> StringToTypes = new();
    public static HashSet<ArrowTypes> LowPriorityTypes = new();

    public static void AddArrow(IArrowEntry entry)
    {
        arrowEntries[entry.Name] = entry;
        typesToArrowEntries[entry.ArrowTypes] = entry;
    }

#nullable enable
    public static IArrowEntry? GetArrow(string id)
    {
        arrowEntries.TryGetValue(id, out var entry);
        return entry;
    }

    public static IArrowEntry? GetArrow(ArrowTypes type)
    {
        typesToArrowEntries.TryGetValue(type, out var entry);
        return entry;
    }

    internal static IReadOnlyDictionary<string, IArrowEntry> GetArrowEntries()
    {
        return arrowEntries;
    }

    internal static patch_Arrow? CreateArrow(ArrowTypes types)
    {
        var arrowEntry = GetArrow(types);

        if (arrowEntry is null)
        {
            RiseCore.logger.LogError("ArrowType cannot be found: " + types);
            return null;
        }

        if (arrowEntry.Configuration.CreateArrow is not null)
        {
            var arrow = arrowEntry.Configuration.CreateArrow();
            arrow.ArrowType = types;
            return arrow;
        }

        if (arrowEntry.Configuration.ArrowType is null)
        {
            RiseCore.logger.LogError("This type does not exists or it is only for vanilla.");
            return null;
        }


        if (Activator.CreateInstance(arrowEntry.Configuration.ArrowType) is not patch_Arrow arrowT)
        {
            RiseCore.logger.LogError("Invalid arrow type: '{name}'", arrowEntry.Name);
            return null;
        }

        arrowT.ArrowType = types;

        return arrowT;
    }
#nullable disable

    public static void Register(string name, ArrowTypes arrowTypes, in ArrowConfiguration configuration)
    {
        var stride = arrowTypes;

        StringToTypes[name] = stride;

        if (configuration.LowPriority)
        {
            LowPriorityTypes.Add(stride);
        }
    }
}