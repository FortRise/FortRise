using System;

namespace FortRise.Levels;

internal sealed class LevelsModule : Mod
{
    public static LevelsModule Instance = null!;


    public LevelsModule(IModContent content, IModuleContext context) : base(content, context)
    {
        Instance = this;
        context.Events.OnModLoadingFinished += OnModLoadingFinished;
    }

    private void OnModLoadingFinished(object? sender, EventArgs e)
    {
        var dependents = Context.Interop.GetModDependents();
        for (int i = 0; i < dependents.Count; i++)
        {
            var dependent = dependents[i];
            var registry = Context.Interop.GetModRegistry(dependent.Metadata);
            if (registry is null)
            {
                continue;
            }

            ArcherLoader.Load(registry, dependent.Content);
            TilesetLoader.Load(registry, dependent.Content);
            ThemeLoader.Load(registry, dependent.Content);
            VersusLoader.Load(registry, dependent.Content);
            QuestLoader.Load(registry, dependent.Content);
            DarkWorldLoader.Load(registry, dependent.Content);
            TrialsLoader.Load(registry, dependent.Content);
        }
    }
}

internal static class ArcherLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Atlas/GameData/archerData.xml", out IResourceInfo archers))
        {
            return;
        }
    }
}