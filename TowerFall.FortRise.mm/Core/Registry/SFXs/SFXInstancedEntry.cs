#nullable enable
using System;
using Monocle;

namespace FortRise;

internal sealed class SFXInstancedEntry : ISFXInstancedEntry
{
    public string Name { get; init; }
    public IResourceInfo? Path { get; init; } = null!;
    public bool ObeysMasterPitch { get; init; }
    public int Instances { get; init; }

    public patch_SFXInstanced? SFXInstanced => GetActualSFXInstanced();

    public SFX? BaseSFX => SFXInstanced;
    private Func<patch_SFXInstanced>? sfxCallback;

    public SFXInstancedEntry(string name, IResourceInfo path, int instances, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
        Instances = instances;
    }

    public SFXInstancedEntry(string name, Func<patch_SFXInstanced> callback, int instances, bool obeysMasterPitch)
    {
        Name = name;
        ObeysMasterPitch = obeysMasterPitch;
        Instances = instances;
        sfxCallback = callback;
    }

    private patch_SFXInstanced? cache;
    private patch_SFXInstanced? GetActualSFXInstanced()
    {
        if (cache != null)
        {
            return cache;
        }
        if (sfxCallback != null)
        {
            return cache = sfxCallback();
        }

        using var stream = Path!.Stream;
        return cache = new patch_SFXInstanced(stream, Instances, ObeysMasterPitch);
    }
}
