#nullable enable
using Microsoft.Extensions.Logging;

namespace FortRise;

internal sealed class ModuleContext : IModuleContext
{
    public IModRegistry Registry { get; init; }
    public IModInterop Interop { get; init; }
    public IModEvents Events { get; init; }
    public IModFlags Flags { get; init; }
    public ILogger Logger { get; init; }
    public IHarmony Harmony { get; init; }

    public ModuleContext(IModRegistry registry, IModInterop interop, IModEvents events, IModFlags flags, ILogger logger, IHarmony harmony)
    {
        Registry = registry;
        Interop = interop;
        Events = events;
        Flags = flags;
        Logger = logger;
        Harmony = harmony;
    }
}
