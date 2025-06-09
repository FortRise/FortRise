using MonoMod;

namespace TowerFall;

public class patch_TowerMapData : TowerMapData
{
    public LevelData levelData;

    public patch_TowerMapData(LevelData data) : base(data)
    {
    }

    [MonoModReplace]
    [MonoModConstructor]
    public void ctor(patch_LevelData data)
    {
        levelData = data;
        ID = data.ID;
        TowerTheme theme = data.Theme;
        Title = theme.Name;
        IconTile = theme.TowerType;
        Icon = theme.Icon;
        Position = theme.MapPosition;
        Author = data.Author;
        MaxFFAPlayers = 4;
        AllowTeams = true;
        if (ID.Y == 1)
        {
            Title += " II";
            return;
        }
        if (ID.Y > 1)
        {
            Title += " III";
        }
    }
}