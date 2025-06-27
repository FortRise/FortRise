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
        registryQueue.AddOrInvoke(tileset);
        TilesetsRegistry.AddTileset(tileset);
        return tileset;
    }

    public ITilesetEntry? GetTileset(string id) => TilesetsRegistry.GetTileset(id);
    

    private void Invoke(ITilesetEntry entry)
    {
        var tileset = new patch_TilesetData();
        tileset.Texture = entry.Configuration.Texture.Subtexture;
        tileset.AutotileData = entry.Configuration.AutotileData;

        GameData.Tilesets[entry.Name] = tileset;
    }
}
