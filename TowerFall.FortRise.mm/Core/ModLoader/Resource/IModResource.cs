#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

public interface IModResource : IDisposable
{
    ModuleMetadata Metadata { get; }
    IModContent Content { get; }
    Dictionary<string, IResourceInfo> OwnedResources { get; }

    internal void Lookup(string prefix);
}
