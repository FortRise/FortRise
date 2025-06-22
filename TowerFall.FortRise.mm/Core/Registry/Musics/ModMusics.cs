#nullable enable
using System;
using System.Collections.Generic;
using Monocle;

namespace FortRise;

public class ModMusics
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IMusicEntry> queue;
    private readonly Dictionary<string, IMusicEntry> entries = new();

    internal ModMusics(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        queue = manager.CreateQueue<IMusicEntry>(Invoke);
    }

    public IMusicEntry RegisterMusic(string id, IResourceInfo filePath)
    {
        string name = $"{metadata.Name}/{id}";
        IMusicEntry entry = new MusicEntry(name, filePath);
        entries.Add(name, entry);
        queue.AddOrInvoke(entry);
        return entry;
    }

    public IMusicEntry? GetMusic(string id)
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out IMusicEntry? value);
        return value;
    }

    private void Invoke(IMusicEntry entry)
    {
        var trackInfo = new TrackInfo(entry.Name, entry.MusicPath);
        patch_Audio.TrackMap[entry.Name] = trackInfo;
    }
}