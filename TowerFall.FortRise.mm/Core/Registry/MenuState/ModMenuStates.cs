#nullable enable
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public interface IModMenuStates
{
    IMenuStateEntry RegisterMenuState(string id, in MenuStateConfiguration configuration);
    IMenuStateEntry? GetMenuState(string id);
}


internal sealed class ModMenuStates : IModMenuStates
{
    private readonly RegistryQueue<IMenuStateEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModMenuStates(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IMenuStateEntry>(Invoke);
    }

    public IMenuStateEntry RegisterMenuState(string id, in MenuStateConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";
        IMenuStateEntry menuState = new MenuStateEntry(name, configuration, EnumPool.Obtain<MainMenu.MenuState>());
        CustomMenuStateRegistry.AddMenuState(menuState);
        registryQueue.AddOrInvoke(menuState);
        return menuState;
    }

    public IMenuStateEntry? GetMenuState(string id)
    {
        return CustomMenuStateRegistry.GetMenuState(id);
    }

    internal void Invoke(IMenuStateEntry entry)
    {
        CustomMenuStateRegistry.Register(entry.Name, entry.MenuState, entry.Configuration);
    }
}