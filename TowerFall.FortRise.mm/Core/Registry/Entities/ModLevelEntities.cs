#nullable enable
namespace FortRise;

public interface IModLevelEntities
{
    ILevelEntityEntry? GetLevelEntity(string id);
    ILevelEntityEntry RegisterLevelEntity(string id, in LevelEntityConfiguration configuration);
}

internal sealed class ModLevelEntities : IModLevelEntities
{
    private readonly RegistryQueue<ILevelEntityEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModLevelEntities(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<ILevelEntityEntry>(Invoke);
    }

    public ILevelEntityEntry RegisterLevelEntity(string id, in LevelEntityConfiguration configuration)
    {
        string name = $"{metadata.Name}.{id}";

        ILevelEntityEntry enemy = new LevelEntityEntry(name, configuration);
        EntityRegistry.AddLevelEntity(enemy);
        registryQueue.AddOrInvoke(enemy);
        return enemy;
    }

    public ILevelEntityEntry? GetLevelEntity(string id)
    {
        return EntityRegistry.GetLevelEntity(id);
    }

    internal void Invoke(ILevelEntityEntry entry)
    {
        EntityRegistry.AddLevelEntity(entry.ID, entry.Configuration);
    }
}
