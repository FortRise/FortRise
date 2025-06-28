using System;
using Microsoft.Extensions.Logging;

namespace FortRise.Content;

internal sealed class LevelsModule : Mod
{
    public static LevelsModule Instance = null!;


    public LevelsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Events.OnModLoadStateFinished += OnMoadLoadStateFinished;
    }

    private void OnMoadLoadStateFinished(object? sender, LoadState e)
    {
        if (e != LoadState.Load)
        {
            return;
        }

        var dependents = Context.Interop.GetModDependents();
        for (int i = 0; i < dependents.Count; i++)
        {
            var dependent = dependents[i];
            var registry = Context.Interop.GetModRegistry(dependent.Metadata);
            if (registry is null)
            {
                continue;
            }

            SpriteDataLoader.LoadSpriteData(registry, dependent.Content, ContainerSpriteType.Main);
            SpriteDataLoader.LoadSpriteData(registry, dependent.Content, ContainerSpriteType.Menu);
            SpriteDataLoader.LoadSpriteData(registry, dependent.Content, ContainerSpriteType.BG);
            SpriteDataLoader.LoadSpriteData(registry, dependent.Content, ContainerSpriteType.Boss);
            SpriteDataLoader.LoadSpriteData(registry, dependent.Content, ContainerSpriteType.Corpse);
            MusicLoader.Load(registry, dependent.Content);
            ArcherLoader.Load(registry, dependent.Content);
            TilesetLoader.Load(registry, dependent.Content);
            ThemeLoader.Load(registry, dependent.Content);
            VersusLoader.Load(registry, dependent.Content, Logger);
            QuestLoader.Load(registry, dependent.Content);
            DarkWorldLoader.Load(registry, dependent.Content);
            TrialsLoader.Load(registry, dependent.Content, Logger);
        }
    }
}
