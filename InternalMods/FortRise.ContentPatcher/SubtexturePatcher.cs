using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise.ContentPatcher;

internal sealed class SubtexturePatcher
{
    private readonly Dictionary<(SubtextureAtlasDestination, string), List<PatchHandle<IFortRiseContentPatcherAPI.ISubtexturePatch>>> replacements = new();

    public PatchHandle<IFortRiseContentPatcherAPI.ISubtexturePatch> Register(ModuleMetadata owner, IFortRiseContentPatcherAPI.ISubtexturePatch patch)
    {
        var key = (patch.Atlas, patch.Target);
        var handle = new PatchHandle<IFortRiseContentPatcherAPI.ISubtexturePatch>(owner, patch, patch.Id, h => Remove(key, h));
        if (!replacements.TryGetValue(key, out var list))
        {
            list = [];
            replacements[key] = list;
        }
        list.Add(handle);
        return handle;
    }

    public void RemoveAll(ModuleMetadata owner)
    {
        var emptyKeys = new List<(SubtextureAtlasDestination, string)>();
        foreach (var (key, list) in replacements)
        {
            list.RemoveAll(h => h.Owner == owner);
            if (list.Count == 0)
            {
                emptyKeys.Add(key);
            }
        }

        foreach (var key in emptyKeys)
        {
            replacements.Remove(key);
        }
    }

    private void Remove((SubtextureAtlasDestination, string) key, PatchHandle<IFortRiseContentPatcherAPI.ISubtexturePatch> handle)
    {
        if (replacements.TryGetValue(key, out var list) && list.Remove(handle) && list.Count == 0)
        {
            replacements.Remove(key);
        }
    }

    public void OnRequestingSubtexture(object? sender, RequestingSubtextureEventArgs e)
    {
        if (replacements.Count == 0 || sender is not Atlas atlas)
        {
            return;
        }

        if (!TryGetDestination(atlas, out var destination))
        {
            return;
        }

        if (!replacements.TryGetValue((destination, e.Name), out var list))
        {
            return;
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var handle = list[i];
            if (handle.Enabled && handle.Patch.GetSubtexture() is { } subtexture)
            {
                e.Edit(() => subtexture);
                return;
            }
        }
    }

    private static bool TryGetDestination(Atlas atlas, out SubtextureAtlasDestination destination)
    {
        if (atlas == TFGame.Atlas)
        {
            destination = SubtextureAtlasDestination.Atlas;
            return true;
        }
        if (atlas == TFGame.BGAtlas)
        {
            destination = SubtextureAtlasDestination.BGAtlas;
            return true;
        }
        if (atlas == TFGame.MenuAtlas)
        {
            destination = SubtextureAtlasDestination.MenuAtlas;
            return true;
        }
        if (atlas == TFGame.BossAtlas)
        {
            destination = SubtextureAtlasDestination.BossAtlas;
            return true;
        }

        destination = default;
        return false;
    }
}

internal sealed record EntrySubtexturePatch(
    string Id, SubtextureAtlasDestination Atlas, string Target, ISubtextureEntry Entry, PatchCondition? Condition, LevelPatcher LevelPatcher)
    : IFortRiseContentPatcherAPI.ISubtexturePatch
{
    public Subtexture? GetSubtexture()
    {
        if (Condition is not null && !Condition.Matches(LevelPatcher.GetCurrentLevel()))
        {
            return null;
        }
        return Entry.Subtexture;
    }
}
