using System;
using System.Collections.Generic;

namespace FortRise;

internal abstract class RegistryQueue 
{
    internal abstract void Invoke();
}


internal class RegistryQueue<T> : RegistryQueue
where T : class
{
    private List<T> queues = new List<T>();
    private ModuleManager.LoadState loadState;
    private Action<T> invoker;

    public RegistryQueue(ModuleManager manager, Action<T> invoker)
    {
        this.loadState = manager.State;
        this.invoker = invoker;
    }

    internal override void Invoke()
    {
        foreach (var queue in queues)
        {
            invoker(queue);
        }
        queues.Clear();
    }

    internal void AddOrInvoke(T entry)
    {
        if (loadState == ModuleManager.LoadState.Ready)
        {
            invoker(entry);
            return;
        }

        queues.Add(entry);
    }
}