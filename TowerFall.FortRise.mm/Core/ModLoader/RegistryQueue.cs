using System;
using System.Collections.Generic;

namespace FortRise;

internal enum RegistryBatchType
{
    PreloadedContent,
    Initialization
}

internal abstract class RegistryQueue 
{
    internal abstract void Invoke();
}

internal sealed class PreparedRegistryQueue<T> : RegistryQueue
where T : class
{
    private List<T> queues = new List<T>();
    private LoadState loadState;
    private Action<T> invoker;
    private Action<List<T>> prepare;

    public PreparedRegistryQueue(ModuleManager manager, Action<List<T>> prepare, Action<T> invoker)
    {
        this.loadState = manager.State;
        this.invoker = invoker;
        this.prepare = prepare;
    }

    internal override void Invoke()
    {
        prepare(queues);

        foreach (var queue in queues)
        {
            invoker(queue);
        }
        queues.Clear();
        loadState = LoadState.Ready;
    }

    internal void AddOrInvoke(T entry)
    {
        queues.Add(entry);

        if (loadState == LoadState.Ready)
        {
            prepare(queues);
            invoker(entry);
            queues.Clear();
        }
    }
}

internal class RegistryQueue<T> : RegistryQueue
where T : class
{
    private List<T> queues = new List<T>();
    private LoadState loadState;
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
        loadState = LoadState.Ready;
    }

    internal void AddOrInvoke(T entry)
    {
        if (loadState == LoadState.Ready)
        {
            invoker(entry);
            return;
        }

        queues.Add(entry);
    }
}