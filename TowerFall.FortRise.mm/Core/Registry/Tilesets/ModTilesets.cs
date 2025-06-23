#nullable enable
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModTilesets
{
    ITilesetEntry RegisterTileset(string id, TilesetConfiguration configuration);
    ITilesetEntry? GetTileset(string id);
}

internal sealed class ModTilesets : IModTilesets
{
    private readonly Dictionary<string, ITilesetEntry> entries = new Dictionary<string, ITilesetEntry>();
    private readonly RegistryQueue<ITilesetEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModTilesets(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ITilesetEntry>(Invoke);
    }

    public ITilesetEntry RegisterTileset(string id, TilesetConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        ITilesetEntry tileset = new TilesetEntry(name, configuration);
        entries.Add(name, tileset);
        registryQueue.AddOrInvoke(tileset);
        return tileset;
    }

    public ITilesetEntry? GetTileset(string id)
    {
        ReadOnlySpan<char> name = $"{metadata.Name}/{id}";
        var alternate = entries.GetAlternateLookup<ReadOnlySpan<char>>();
        alternate.TryGetValue(name, out ITilesetEntry? value);
        return value;
    }

    private void Invoke(ITilesetEntry entry)
    {
        var tileset = new patch_TilesetData();
        tileset.Texture = entry.Configuration.Texture.Subtexture;
        tileset.AutotileData = entry.Configuration.AutotileData;

        GameData.Tilesets[entry.Name] = tileset;
    }
}