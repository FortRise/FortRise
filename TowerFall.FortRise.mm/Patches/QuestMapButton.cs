using System;
using FortRise;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.QuestMapButton")]
public class QuestMapButton : TowerFall.QuestMapButton
{
    public QuestMapButton(QuestLevelData level) : base(level)
    {
    }

    [MonoModReplace]
    protected override bool GetLocked()
    {
        var level = GameData.QuestLevels[Data.ID.X];
        if (TowerRegistry.QuestTowers.TryGetValue(level.LevelID, out var entry))
        {
            var locked = entry.Configuration.ShowLocked?.Invoke(entry);
            if (locked is {} l)
            {
                return l;
            }

            return false;
        }
        return !SaveData.Instance.Quest.Towers[base.Data.ID.X].Revealed;
    }
}