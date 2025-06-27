#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace FortRise;

public interface IModEffects
{
    IEffectEntry RegisterEffect(string id, EffectConfiguration configuration);
}

internal sealed class ModEffects : IModEffects
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IEffectEntry> queue;
    private readonly Dictionary<string, IEffectEntry> entries = new();

    internal ModEffects(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        queue = manager.CreateQueue<IEffectEntry>(Invoke);
    }

    public IEffectEntry RegisterEffect(string id, EffectConfiguration configuration)
    {
        var name = $"{metadata.Name}/{id}";

        var entry = new EffectEntry(name, configuration);
        queue.AddOrInvoke(entry);
        entries.Add(name, entry);
        return entry;
    }

    private void Invoke(IEffectEntry entry)
    {
        if (!entry.Configuration.EffectResourceType.IsCompatible(typeof(EffectResource)))
        {
            Logger.Error($"Effect type '{entry.Configuration.EffectResourceType.Name}' from '{entry.ID}' is not a base type of 'Effect Resource'");
            return;
        }

        using var fxStream = entry.Configuration.EffectFile.Stream;
        using var memStream = new MemoryStream();
        fxStream.CopyTo(memStream);
        var effect = new Effect(Engine.Instance.GraphicsDevice, memStream.ToArray());
        var shaderResource = (EffectResource)Activator.CreateInstance(entry.Configuration.EffectResourceType)!;
        shaderResource.Init(effect, entry.Configuration.PassName);
        EffectManager.Shaders.Add(entry.ID, shaderResource);
    }
}