#nullable enable
using Microsoft.Extensions.Logging;

namespace FortRise;

public interface IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public IModFlags Flags { get; init; }
    public ILogger Logger { get; init; }
    public IHarmony Harmony { get; init; }
}
