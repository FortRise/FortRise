using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Monocle;
using TowerFall;

namespace FortRise.ContentPatcher;

internal sealed class LevelPatcher(ILogger logger)
{
    private static LevelPatcher instance = null!;

    private readonly List<PatchHandle<IFortRiseContentPatcherAPI.ILevelPatch>> patches = [];

    public PatchedLevel? LastPatched { get; private set; }

    private CurrentLevel? lastLoaded;

    public CurrentLevel? GetCurrentLevel()
    {
        XmlElement? xml = Engine.Instance?.Scene switch
        {
            Level level => level.XML,
            LevelLoaderXML loader => loader.XML,
            _ => null
        };

        return xml is not null && lastLoaded is { } current && ReferenceEquals(current.Xml, xml) ? current : null;
    }

    public void Hook(IHarmony harmony)
    {
        instance = this;
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(VersusLevelSystem), nameof(VersusLevelSystem.GetNextRoundLevel)),
            postfix: new HarmonyMethod(VersusLevelSystem_GetNextRoundLevel_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DarkWorldLevelSystem), nameof(DarkWorldLevelSystem.GetNextRoundLevel)),
            postfix: new HarmonyMethod(DarkWorldLevelSystem_GetNextRoundLevel_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(QuestLevelSystem), nameof(QuestLevelSystem.GetNextRoundLevel)),
            postfix: new HarmonyMethod(QuestLevelSystem_GetNextRoundLevel_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TrialsLevelSystem), nameof(TrialsLevelSystem.GetNextRoundLevel)),
            postfix: new HarmonyMethod(TrialsLevelSystem_GetNextRoundLevel_Postfix)
        );
    }

    public void RemoveAll(ModuleMetadata owner)
    {
        patches.RemoveAll(p => p.Owner == owner);
    }

    public PatchHandle<IFortRiseContentPatcherAPI.ILevelPatch> Register(ModuleMetadata owner, IFortRiseContentPatcherAPI.ILevelPatch patch)
    {
        var handle = new PatchHandle<IFortRiseContentPatcherAPI.ILevelPatch>(owner, patch, patch.Id, h => patches.Remove(h));
        patches.Add(handle);
        return handle;
    }

    public bool Add(ModuleMetadata owner, PatchDefinition definition)
    {
        if (string.IsNullOrEmpty(definition.Target))
        {
            logger.LogError("[{mod}] '{id}': PatchLevel requires 'target'.", owner.Name, definition.Id);
            return false;
        }

        if (!PatchCondition.TryParse(logger, owner.Name, definition.Id, definition.Where, out var condition))
        {
            return false;
        }

        var changes = new List<LevelChange>();
        foreach (var changeElement in definition.Changes ?? [])
        {
            if (!TryParseChange(owner, definition.Id, changeElement, out var change))
            {
                return false;
            }
            changes.Add(change);
        }

        Register(owner, new JsonLevelPatch(definition.Id, definition.Target, condition, changes));
        return true;
    }

    private bool TryParseChange(ModuleMetadata owner, string id, JsonElement element, out LevelChange change)
    {
        change = new LevelChange();
        if (element.ValueKind != JsonValueKind.Object)
        {
            logger.LogError("[{mod}] '{id}': Each change must be an object.", owner.Name, id);
            return false;
        }

        foreach (var prop in element.EnumerateObject())
        {
            switch (prop.Name)
            {
                case "Attributes" when prop.Value.ValueKind == JsonValueKind.Object:
                    foreach (var attr in prop.Value.EnumerateObject())
                    {
                        change.Attributes[attr.Name] = AsString(attr.Value);
                    }
                    break;
                case "Attributes":
                    logger.LogError("[{mod}] '{id}': 'Attributes' must be an object.", owner.Name, id);
                    return false;
                default:
                    switch (prop.Value.ValueKind)
                    {
                        case JsonValueKind.String:
                            change.Layers[prop.Name] = prop.Value.GetString()!;
                            break;
                        case JsonValueKind.Array:
                            if (!change.EntityLayers.TryGetValue(prop.Name, out var ops))
                            {
                                ops = [];
                                change.EntityLayers[prop.Name] = ops;
                            }
                            foreach (var op in prop.Value.EnumerateArray())
                            {
                                if (!TryParseEntityOperation(owner, id, op, out var entityOp))
                                {
                                    return false;
                                }
                                ops.Add(entityOp);
                            }
                            break;
                        default:
                            logger.LogError("[{mod}] '{id}': Layer '{layer}' must be a string or an array of entity operations.", owner.Name, id, prop.Name);
                            return false;
                    }
                    break;
            }
        }

        return true;
    }

    private bool TryParseEntityOperation(ModuleMetadata owner, string id, JsonElement element, out EntityOperation operation)
    {
        operation = default!;
        if (!element.TryGetProperty("type", out var typeElement) || !element.TryGetProperty("value", out var value))
        {
            logger.LogError("[{mod}] '{id}': Entity operations require 'type' and 'value'.", owner.Name, id);
            return false;
        }

        switch (typeElement.GetString())
        {
            case "RemoveByID":
                operation = new EntityOperation.RemoveByID(AsString(value));
                return true;
            case "Add" when value.ValueKind == JsonValueKind.Object:
                string? name = null;
                var attributes = new Dictionary<string, string>();
                foreach (var prop in value.EnumerateObject())
                {
                    if (prop.Name == "name")
                    {
                        name = AsString(prop.Value);
                        continue;
                    }
                    attributes[prop.Name] = AsString(prop.Value);
                }

                if (string.IsNullOrEmpty(name))
                {
                    logger.LogError("[{mod}] '{id}': Add operation requires a 'name'.", owner.Name, id);
                    return false;
                }

                operation = new EntityOperation.Add(name, attributes);
                return true;
            default:
                logger.LogError("[{mod}] '{id}': Unknown entity operation '{type}'.", owner.Name, id, typeElement.ToString());
                return false;
        }
    }

    private void Apply(LevelSystem levelSystem, MatchSettings matchSettings, XmlElement? level, string? levelPath)
    {
        if (level is null || levelSystem.Theme is not { } theme)
        {
            return;
        }

        var current = new CurrentLevel(level, levelSystem, theme, levelPath, matchSettings.Mode);
        lastLoaded = current;
        if (patches.Count == 0)
        {
            return;
        }

        var context = new LevelContext(current);
        XmlElement? original = null;
        var applied = new List<string>();
        foreach (var handle in patches.ToArray())
        {
            if (!handle.Enabled)
            {
                continue;
            }

            try
            {
                if (handle.Patch.CanPatch(context))
                {
                    original ??= (XmlElement)level.CloneNode(true);
                    applied.Add($"{handle.Owner.Name}/{handle.Id}");
                    handle.Patch.Patch(context);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("[{mod}] '{id}': Failed to patch level '{level}': {ex}", handle.Owner.Name, handle.Id, levelPath, ex);
            }
        }

        LastPatched = original is null ? null : new PatchedLevel(level, original, theme.ID, levelPath, applied);
    }

    private static string AsString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.True => "True",
            JsonValueKind.False => "False",
            _ => element.GetRawText()
        };
    }

    private static void VersusLevelSystem_GetNextRoundLevel_Postfix(VersusLevelSystem __instance, MatchSettings matchSettings, XmlElement __result)
    {
        instance.Apply(__instance, matchSettings, __result, __instance.LastLevel);
    }

    private static void DarkWorldLevelSystem_GetNextRoundLevel_Postfix(
        DarkWorldLevelSystem __instance, MatchSettings matchSettings, int roundIndex, XmlElement __result)
    {
        string? path = null;
        try
        {
            var tower = __instance.DarkWorldTowerData;
            int file = tower[matchSettings.DarkWorldDifficulty][roundIndex + __instance.startLevel].File;
            path = tower.Levels[file];
        }
        catch (ArgumentOutOfRangeException) {}

        instance.Apply(__instance, matchSettings, __result, path);
    }

    private static void QuestLevelSystem_GetNextRoundLevel_Postfix(QuestLevelSystem __instance, MatchSettings matchSettings, XmlElement __result)
    {
        instance.Apply(__instance, matchSettings, __result, __instance.QuestTowerData.Path);
    }

    private static void TrialsLevelSystem_GetNextRoundLevel_Postfix(TrialsLevelSystem __instance, MatchSettings matchSettings, XmlElement __result)
    {
        instance.Apply(__instance, matchSettings, __result, __instance.TrialsLevelData.Path);
    }
}

internal sealed class CurrentLevel(XmlElement xml, LevelSystem levelSystem, TowerTheme theme, string? levelPath, Modes mode)
    : IFortRiseContentPatcherAPI.ILevelInfo
{
    public Modes Mode { get; } = mode;
    public string ModeName { get; } = GameModeRegistry.ModesToVersusGameMode.TryGetValue(mode, out var entry) ? entry.Name : mode.ToString();
    public XmlElement Xml { get; } = xml;
    public LevelSystem LevelSystem { get; } = levelSystem;
    public TowerTheme Theme { get; } = theme;
    public string? LevelPath { get; } = levelPath;
    public string? Filename { get; } = levelPath is null ? null : Path.GetFileName(levelPath);

    public bool IsTower(string idOrName)
    {
        return string.Equals(idOrName, Theme.ID, StringComparison.OrdinalIgnoreCase)
            || string.Equals(idOrName, Theme.Name, StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed record PatchedLevel(XmlElement Patched, XmlElement Original, string Tower, string? LevelPath, List<string> AppliedPatches);

internal sealed record JsonLevelPatch(string Id, string Target, PatchCondition? Condition, List<LevelChange> Changes)
    : IFortRiseContentPatcherAPI.ILevelPatch
{
    public bool CanPatch(IFortRiseContentPatcherAPI.ILevelContext context)
    {
        return context.IsTower(Target) && (Condition is null || Condition.Matches(context));
    }

    public void Patch(IFortRiseContentPatcherAPI.ILevelContext context)
    {
        foreach (var change in Changes)
        {
            foreach (var (key, value) in change.Attributes)
            {
                context.SetAttribute(key, value);
            }

            foreach (var (layer, value) in change.Layers)
            {
                context.SetLayerText(layer, value);
            }

            foreach (var (layer, ops) in change.EntityLayers)
            {
                context.GetOrCreateLayer(layer);
                foreach (var op in ops)
                {
                    switch (op)
                    {
                        case EntityOperation.RemoveByID remove:
                            context.RemoveEntityByID(layer, remove.ID);
                            break;
                        case EntityOperation.Add add:
                            context.AddEntity(layer, add.Name, add.Attributes);
                            break;
                    }
                }
            }
        }
    }
}

internal sealed class LevelChange
{
    public Dictionary<string, string> Attributes { get; } = [];
    public Dictionary<string, string> Layers { get; } = [];
    public Dictionary<string, List<EntityOperation>> EntityLayers { get; } = [];
}

internal abstract record EntityOperation
{
    public sealed record RemoveByID(string ID) : EntityOperation;
    public sealed record Add(string Name, Dictionary<string, string> Attributes) : EntityOperation;
}
