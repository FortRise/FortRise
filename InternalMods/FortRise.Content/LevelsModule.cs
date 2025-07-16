using Microsoft.Extensions.Logging;

namespace FortRise.Content;

internal sealed class LevelsModule : Mod
{
    public static LevelsModule Instance = null!;


    public LevelsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Events.OnBeforeModInstantiation += OnBeforeModInstantiation;
    }

    private void OnBeforeModInstantiation(object? sender, BeforeModInstantiationEventArgs e)
    {
        if (!e.Context.Interop.IsModDepends(ModContent.Metadata))
        {
            return;
        }

        var content = e.ModContent;
        var registry = e.Context.Registry;
        if (registry is null)
        {
            return;
        }

        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Main);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Menu);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.BG);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Boss);
        SpriteDataLoader.LoadSpriteData(registry, content, ContainerSpriteType.Corpse);
        MusicLoader.Load(registry, content);
        ArcherLoader.Load(registry, content);
        TilesetLoader.Load(registry, content);
        ThemeLoader.Load(registry, content);
        VersusLoader.Load(registry, content, Logger);
        QuestLoader.Load(registry, content);
        DarkWorldLoader.Load(registry, content);
        TrialsLoader.Load(registry, content, Logger);
    }
}
