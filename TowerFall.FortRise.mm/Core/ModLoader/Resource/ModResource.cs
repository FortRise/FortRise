using System;
using System.Collections.Generic;

namespace FortRise;

public abstract class ModResource : IModResource
{
    public Dictionary<string, IResourceInfo> Resources = new();
    private bool disposedValue;

    public ModuleMetadata Metadata { get; private set; }
    public IModContent Content { get; private set; }
    public Dictionary<string, IResourceInfo> OwnedResources => Resources;


    public ModResource(ModuleMetadata metadata, IModContent content)
    {
        Metadata = metadata;
        Content = content;
    }


    public void Add(string path, IResourceInfo resource)
    {
        var rootName = (Metadata is not null ? Metadata.Name : "::global::");
        var rootPath = resource.Root = $"mod:{rootName}/";

        Logger.Verbose("[RESOURCE] Loaded On:" + rootPath);
        Logger.Verbose("[RESOURCE] Loaded:" + path);
        if (Resources.ContainsKey(path))
            return;


        Resources.Add(path, resource);
        RiseCore.ResourceTree.TreeMap.Add($"{rootPath}{path}", resource);
    }


    public abstract void Lookup(string prefix);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                DisposeInternal();
            }

            disposedValue = true;
        }
    }

    internal virtual void DisposeInternal() {}

    ~ModResource()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
