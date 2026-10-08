using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FortRise;

internal sealed class RegistryUnloader<T>(Action<T> invoker, RegistryQueue<T> queue) : RegistryUnloader
where T : class
{
    private readonly Action<T> invoker = invoker;
    private readonly IList<T> contents = queue.ContentQueues;

    public override void Unload()
    {
        foreach (var c in contents)
        {
            try
            {
                invoker(c);
            }
            catch (Exception ex)
            {
                RiseCore.logger.LogError(ex, "Failed to detach registry entry '{entry}'.", c);
            }
        }
    }
}

internal abstract class RegistryUnloader
{
    public abstract void Unload();
}

