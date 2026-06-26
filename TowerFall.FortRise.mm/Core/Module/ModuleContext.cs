#nullable enable
using Microsoft.Extensions.Logging;

namespace FortRise;

internal sealed class ModuleContext(
    IModRegistry registry, 
    IModInterop interop, 
    IModEvents events, 
    IModFlags flags, 
    IModStorage storage, 
    IModEnvironment environment, 
    ILogger logger, 
    IHarmony harmony) : IModuleContext
{
    public IModRegistry Registry { get; init; } = registry;
    public IModInterop Interop { get; init; } = interop;
    public IModEvents Events { get; init; } = events;
    public IModFlags Flags { get; init; } = flags;
    public IModStorage Storage { get; init; } = storage;
    public IModEnvironment Environment { get; init; } = environment;
    public ILogger Logger { get; init; } = logger;
    public IHarmony Harmony { get; init; } = harmony;
}
