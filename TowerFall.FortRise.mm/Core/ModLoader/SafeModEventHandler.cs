#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public sealed class SafeModEventHandler<TEventArgs>
{
    private enum DelayType
    {
        Add,
        Remove
    }

    private List<SafeEventHandler> safeHandlers = new();
    private List<(DelayType type, SafeEventHandler handler)> delayedHandlers = new();
    private bool isRaising;


    public void Add(ModuleMetadata metadata, EventHandler<TEventArgs> handler)
    {
        lock (safeHandlers)
        {
            SafeEventHandler safeEventHandler = new SafeEventHandler(metadata, handler);
            if (isRaising)
            {
                delayedHandlers.Add((DelayType.Add, safeEventHandler));
            }
            else
            {
                Add(safeEventHandler);
            }
        }
    }

    private void Add(in SafeEventHandler handler)
    {
        lock (safeHandlers)
        {
            safeHandlers.Add(handler);
        }
    }

    public void Remove(ModuleMetadata metadata, EventHandler<TEventArgs> handler)
    {
        lock (safeHandlers)
        {
            SafeEventHandler safeEventHandler = new SafeEventHandler(metadata, handler);
            if (isRaising)
            {
                delayedHandlers.Add((DelayType.Remove, safeEventHandler));
            }
            else
            {
                Remove(safeEventHandler);
            }
        }
    }

    private void Remove(in SafeEventHandler handler)
    {
        lock (safeHandlers)
        {
            safeHandlers.Remove(handler);
        }
    }

    public TEventArgs Raise(object? sender, TEventArgs args)
    {
        lock (safeHandlers)
        {
            isRaising = true;
            try
            {
                foreach (var handler in safeHandlers)
                {
                    handler.Handler?.Invoke(sender, args);
                }
            }
            finally
            {
                isRaising = false;

                foreach (var (type, handler) in delayedHandlers)
                {
                    switch (type)
                    {
                        case DelayType.Add:
                            Add(handler);
                            break;
                        case DelayType.Remove:
                            Remove(handler);
                            break;
                    }
                }

                delayedHandlers.Clear();
            }

            return args;
        }
    }

    private record struct SafeEventHandler(ModuleMetadata Metadata, EventHandler<TEventArgs> Handler);
}
