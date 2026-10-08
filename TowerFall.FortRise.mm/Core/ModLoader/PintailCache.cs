#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nanoray.Pintail;

namespace FortRise;

// Pintail does not have a public API to remove its caches. So this touches the proxy manager context to drop everything on an unloaded mod.

internal static class PintailCache
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly FieldInfo? factoriesField = typeof(ProxyManager<string>).GetField("Factories", Flags);
    private static readonly FieldInfo? failedField = typeof(ProxyManager<string>).GetField("FailedProxyTypeExceptions", Flags);
    private static readonly FieldInfo? mappabilityField = typeof(ProxyManager<string>).GetField("InterfaceMappabilityCache", Flags);

    private static bool warned;

    public static void RemoveMod(IProxyManager<string> proxyManager, string modName, ILogger logger)
    {
        if (proxyManager is not ProxyManager<string> manager
            || factoriesField?.GetValue(manager) is not Dictionary<ProxyInfo<string>, IProxyFactory<string>> factories)
        {
            if (!warned)
            {
                warned = true;
                logger.LogWarning("Could not access Pintail's proxy cache, proxies of unloaded mods will not be released.");
            }
            return;
        }

        lock (factories)
        {
            RemoveKeys(factories, modName);
        }

        if (failedField?.GetValue(manager) is Dictionary<ProxyInfo<string>, Exception> failed)
        {
            lock (failed)
            {
                RemoveKeys(failed, modName);
            }
        }

        // Keyed by type names, so entries can't be attributed to a mod. It's only a lookup cache, rebuilt on demand.
        if (mappabilityField?.GetValue(manager) is IDictionary mappability)
        {
            mappability.Clear();
        }
    }

    private static void RemoveKeys<TValue>(Dictionary<ProxyInfo<string>, TValue> dictionary, string modName)
    {
        List<ProxyInfo<string>> toRemove = [];

        foreach (var info in dictionary.Keys)
        {
            if (info.Target.Context == modName || info.Proxy.Context == modName)
            {
                toRemove.Add(info);
            }
        }

        foreach (var info in toRemove)
        {
            dictionary.Remove(info);
        }
    }
}
