using System;
using Microsoft.Extensions.Logging;

namespace FortRise.ContentPatcher;

internal sealed class ContentPatcherModule : Mod
{
    private readonly PatchLoader loader;
    private readonly SubtexturePatcher subtexturePatcher = new();
    private readonly LevelPatcher levelPatcher;

    public ContentPatcherModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        loader = new PatchLoader(logger);
        levelPatcher = new LevelPatcher(logger);
        levelPatcher.Hook(context.Harmony);
        DumpPatchCommand.Register(context.Registry, levelPatcher);
        context.Events.Mods.BeforeInstantiation += OnBeforeModInstantiation;
        context.Events.Atlas.RequestingSubtexture += subtexturePatcher.OnRequestingSubtexture;
    }

    public override object? GetApi() => new ApiImplementation(subtexturePatcher, levelPatcher);

    private void OnBeforeModInstantiation(object? sender, BeforeModInstantiationEventArgs e)
    {
        if (!e.Context.Interop.IsModDepends(ModContent.Metadata))
        {
            return;
        }

        var content = e.ModContent;
        var registry = e.Context.Registry;
        if (registry is null || !content.Root.ExistsRelativePath("patchContent.json"))
        {
            return;
        }

        var metadata = content.Metadata;
        subtexturePatcher.RemoveAll(metadata);
        levelPatcher.RemoveAll(metadata);

        foreach (var patch in loader.Load(content, "patchContent.json"))
        {
            switch (patch.Type)
            {
                case "ReplaceSubtexture":
                    ApplyReplaceSubtexture(content, registry, patch);
                    break;
                case "PatchLevel":
                    levelPatcher.Add(metadata, patch);
                    break;
                default:
                    Logger.LogError("[{mod}] '{id}': Unknown patch type '{type}'.", metadata.Name, patch.Id, patch.Type);
                    break;
            }
        }
    }

    private void ApplyReplaceSubtexture(IModContent content, IModRegistry registry, PatchDefinition patch)
    {
        var mod = content.Metadata.Name;
        if (string.IsNullOrEmpty(patch.Target) || string.IsNullOrEmpty(patch.Replace))
        {
            Logger.LogError("[{mod}] '{id}': ReplaceSubtexture requires 'target' and 'replace'.", mod, patch.Id);
            return;
        }

        if (!PatchCondition.TryParse(Logger, mod, patch.Id, patch.Where, out var condition))
        {
            return;
        }

        if (content.Root.TryGetRelativePath(patch.Replace, out var file))
        {
            var entry = registry.Subtextures.RegisterTexture($"ContentPatcher/{patch.Id}", file, patch.Atlas);
            subtexturePatcher.Register(content.Metadata, new EntrySubtexturePatch(patch.Id, patch.Atlas, patch.Target, entry, condition, levelPatcher));
        }
        else
        {
            var entry = registry.Subtextures.GetTexture(patch.Replace, patch.Atlas);
            if (entry is null)
            {
                Logger.LogError("[{mod}] '{id}': File or texture '{file}' not found.", mod, patch.Id, patch.Replace);
                return;
            }
            subtexturePatcher.Register(content.Metadata, new EntrySubtexturePatch(patch.Id, patch.Atlas, patch.Target, entry, condition, levelPatcher));
        }
    }

}

/*
patchContent.json format:

{
    "includes": ["someOtherPatchContentSomewhere.json"],
    "patches": [
    {
        "id": "replace_drillArrow",
        "type": "ReplaceSubtexture",
        "target": "arrows/drillArrow",
        "replace": "Content/Atlas/newDrillArrow.png"
    },
    {
        "id": "patch_id",
        "type": "PatchLevel",
        "target": "Flight",
        "where": {
            "filename": "07.oel"
        },
        "changes": [
        {
            "Attributes": {
                "width": 420
            },
            "Solids": "<ImagineThisIsAStringOfBits>",
            "Entities": [
                {
                    "type": "RemoveByID",
                    "value": "5"
                },
                {
                    "type": "Add",
                    "value": {
                        "name": "TreasureChest",
                        "x": "20",
                        "y": "40",
                        "Mode": "Normal"
                    }
                }
            ]
        }]
    }]
}


*/
