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

internal sealed class PreparedRegistryQueue<T>(
    ModuleManager manager, Action<List<T>> prepare, Action<T> invoker) : RegistryQueue
where T : class
{
    private readonly List<T> queues = [];
    private LoadState loadState = manager.State;
    private readonly Action<T> invoker = invoker;
    private readonly Action<List<T>> prepare = prepare;

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

internal class RegistryQueue<T>(ModuleManager manager, Action<T> invoker) : RegistryQueue
where T : class
{
    private readonly List<T> queues = [];
    private LoadState loadState = manager.State;
    private readonly Action<T> invoker = invoker;

    public IList<T> ContentQueues => queues;

    internal override void Invoke()
    {
        foreach (var queue in queues)
        {
            invoker(queue);
        }
        loadState = LoadState.Ready;
    }

    internal void AddOrInvoke(T entry)
    {
        queues.Add(entry);

        if (loadState == LoadState.Ready)
        {
            invoker(entry);
        }
    }
}
