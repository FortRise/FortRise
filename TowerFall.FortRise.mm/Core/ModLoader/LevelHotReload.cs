#nullable enable
using System.Collections.Generic;
using System.Runtime.Loader;
using Monocle;
using TowerFall;

namespace FortRise;

internal static class LevelHotReload
{
    public static void BeforeUnload(IEnumerable<ModuleMetadata> mods)
    {
        if (Engine.Instance.Scene is not Level level)
        {
            return;
        }

        HashSet<AssemblyLoadContext> contexts = [];
        foreach (var metadata in mods)
        {
            if (metadata.AssemblyLoadContext is { } context)
            {
                contexts.Add(context);
            }
        }

        if (contexts.Count == 0)
        {
            return;
        }

        List<Component> components = [];
        foreach (var layer in level.Layers.Values)
        {
            foreach (var entity in layer.Entities)
            {
                if (IsFrom(entity, contexts))
                {
                    level.Remove(entity);
                    continue;
                }

                components.Clear();
                foreach (var component in entity.Components)
                {
                    if (IsFrom(component, contexts))
                    {
                        components.Add(component);
                    }
                }

                foreach (var component in components)
                {
                    entity.Remove(component);
                }
            }
        }

        if (IsFrom(level.Background, contexts))
        {
            level.Background = null;
        }

        if (IsFrom(level.Foreground, contexts))
        {
            level.Foreground = null;
        }

        level.UpdateEntityLists();
    }

    public static void AfterReload()
    {
        if (Engine.Instance.Scene is not patch_Level level)
        {
            return;
        }

        var session = level.Session;
        var levelSystem = RefreshLevelSystem(session.MatchSettings.LevelSystem);
        if (levelSystem is null)
        {
            Logger.Warning("[Level][Reload] The tower of the current level no longer exists, returning to the main menu.");
            Engine.Instance.Scene = new MainMenu(MainMenu.MenuState.Main);
            return;
        }

        session.MatchSettings.LevelSystem = levelSystem;

        levelSystem.Theme.OnLoad();

        List<Entity> fresh = [];
        Calc.PushRandom(level.LoadSeed);
        try
        {
            // procedural layouts are generated, not read, so only their visuals and archers refresh
            if (!levelSystem.Procedural)
            {
                if (levelSystem is patch_VersusLevelSystem versus)
                {
                    versus.QueueCurrentLevel();
                }

                var xml = levelSystem.GetNextRoundLevel(session.MatchSettings, session.RoundIndex, out _);
                fresh = level.RebuildLayout(xml);
            }

            level.Background = levelSystem.GetBackground(level);
            level.Foreground = levelSystem.GetForeground(level);
            level.RefreshPlayers();
        }
        finally
        {
            Calc.PopRandom();
        }

        level.UpdateEntityLists();

        foreach (var entity in fresh)
        {
            if (entity.Scene == level)
            {
                entity.SceneBegin();
            }
        }

        Logger.Info("[Level][Reload] Refreshed the current level.");
    }

    private static bool IsFrom(object? value, HashSet<AssemblyLoadContext> contexts)
    {
        return value is not null
            && AssemblyLoadContext.GetLoadContext(value.GetType().Assembly) is { } context
            && contexts.Contains(context);
    }

    private static LevelSystem? RefreshLevelSystem(LevelSystem current)
    {
        switch (current)
        {
        case VersusLevelSystem versus:
        {
            var data = Find(GameData.VersusTowers, versus.ID.X, versus.VersusTowerData.LevelID);
            if (data is null)
            {
                return null;
            }

            if (data == versus.VersusTowerData)
            {
                return versus;
            }

            var next = (patch_VersusLevelSystem)data.GetLevelSystem();
            next.ContinueFrom((patch_VersusLevelSystem)versus);
            return next;
        }
        case DarkWorldLevelSystem darkWorld:
        {
            var data = Find(GameData.DarkWorldTowers, darkWorld.ID.X, darkWorld.DarkWorldTowerData.LevelID);
            if (data is null)
            {
                return null;
            }

            if (data == darkWorld.DarkWorldTowerData)
            {
                return darkWorld;
            }

            var next = (patch_DarkWorldLevelSystem)data.GetLevelSystem();
            next.StartLevel = ((patch_DarkWorldLevelSystem)darkWorld).StartLevel;
            return next;
        }
        case QuestLevelSystem quest:
        {
            var data = Find(GameData.QuestLevels, quest.ID.X, quest.QuestTowerData.LevelID);
            if (data is null)
            {
                return null;
            }

            return data == quest.QuestTowerData ? quest : data.GetLevelSystem();
        }
        case TrialsLevelSystem trials:
        {
            var id = trials.TrialsLevelData.ID;
            var levels = GameData.TrialsLevels;
            if (id.X >= levels.GetLength(0) || id.Y >= levels.GetLength(1))
            {
                return null;
            }

            var data = levels[id.X, id.Y];
            if (data is null || data.LevelID != trials.TrialsLevelData.LevelID)
            {
                return null;
            }

            return data == trials.TrialsLevelData ? trials : data.GetLevelSystem();
        }
        default:
            return current;
        }
    }

    private static T? Find<T>(IReadOnlyList<T> list, int index, string levelID)
    where T : LevelData
    {
        if (index < 0 || index >= list.Count)
        {
            return null;
        }

        var data = list[index];
        return data is not null && data.LevelID == levelID ? data : null;
    }
}
