using System.Xml;
using FortRise;
using MonoMod;

namespace TowerFall;

public class patch_LevelSystem : LevelSystem
{
    [MonoModIgnore]
    public extern override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed);

    public extern TilesetData orig_GetTileset();

    public override TilesetData GetTileset() 
    {
        if (RiseCore.GameData.Tilesets.TryGetValue(Theme.Tileset, out var tileset)) 
            return tileset;
        
        return orig_GetTileset();
    }

    public extern TilesetData orig_GetBGTileset();

    public override TilesetData GetBGTileset() 
    {
        if (RiseCore.GameData.Tilesets.TryGetValue(Theme.BGTileset, out var tileset)) 
            return tileset;
        
        return orig_GetBGTileset();
    }

    [MonoModReplace]
    public override Background GetBackground(Level level)
    {
        return new Background(level, this.Theme.BackgroundData);
    }

    [MonoModReplace]
    public override Background GetForeground(Level level)
    {
        if (this.Theme.ForegroundData == null)
        {
            return null;
        }
        
        return new Background(level, this.Theme.ForegroundData);
    }
}