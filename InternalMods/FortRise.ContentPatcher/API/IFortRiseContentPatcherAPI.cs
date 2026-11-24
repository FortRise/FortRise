using System.Collections.Generic;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.ContentPatcher;

public interface IFortRiseContentPatcherAPI
{
    IPatchHandle RegisterLevelPatch(ModuleMetadata owner, ILevelPatch patch);
    IPatchHandle RegisterSubtexturePatch(ModuleMetadata owner, ISubtexturePatch patch);
    void UnregisterAll(ModuleMetadata owner);
    /// <summary>
    /// The level being loaded or played, or null outside of gameplay.
    /// </summary>
    ILevelInfo? GetCurrentLevel();

    public interface IPatchHandle
    {
        string Id { get; }
        bool Enabled { get; set; }
        void Unregister();
    }

    public interface ILevelPatch
    {
        string Id { get; }
        bool CanPatch(ILevelContext context);
        void Patch(ILevelContext context);
    }

    public interface ISubtexturePatch
    {
        string Id { get; }
        SubtextureAtlasDestination Atlas { get; }
        string Target { get; }
        /// <summary>
        /// Called every time the target is requested. Return null to use the original subtexture.
        /// </summary>
        Subtexture? GetSubtexture();
    }

    public interface ILevelInfo
    {
        LevelSystem LevelSystem { get; }
        TowerTheme Theme { get; }
        string? LevelPath { get; }
        string? Filename { get; }
        Modes Mode { get; }
        /// <summary>
        /// The registered name for custom game modes, otherwise the name of <see cref="Mode"/>.
        /// </summary>
        string ModeName { get; }
        bool IsTower(string idOrName);
    }

    public interface ILevelContext : ILevelInfo
    {
        XmlElement Level { get; }
        int Width { get; }
        int Height { get; }
        int Columns { get; }
        int Rows { get; }

        string GetAttribute(string name);
        void SetAttribute(string name, string value);

        XmlElement? GetLayer(string name);
        XmlElement GetOrCreateLayer(string name);
        void RemoveLayer(string name);
        string? GetLayerText(string name);
        void SetLayerText(string name, string text);

        bool[,] GetBitGrid(string layer);
        void SetBitGrid(string layer, bool[,] grid);
        int[,] GetTileGrid(string layer);
        void SetTileGrid(string layer, int[,] grid);

        IReadOnlyList<XmlElement> GetEntities(string layer);
        IReadOnlyList<XmlElement> GetEntities(string layer, string name);
        XmlElement? GetEntityByID(string layer, string id);
        XmlElement AddEntity(string layer, string name, IReadOnlyDictionary<string, string>? attributes = null);
        bool RemoveEntityByID(string layer, string id);
        int RemoveEntities(string layer, string name);
        int NextEntityID(string layer);
    }
}
