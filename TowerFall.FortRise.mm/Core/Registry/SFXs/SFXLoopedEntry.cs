#nullable enable
using System;
using Monocle;

namespace FortRise;

internal sealed class SFXLoopedEntry : ISFXLoopedEntry
{
    public string Name { get; init; }
    public IResourceInfo? Path { get; init; }
    public bool ObeysMasterPitch { get; init; }

    public patch_SFXLooped? SFXLooped => GetActualSFXLooped();

    public SFX? BaseSFX => SFXLooped;
    private Func<patch_SFXLooped>? sfxLoopCallback;

    public SFXLoopedEntry(string name, IResourceInfo path, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
    }

    public SFXLoopedEntry(string name, Func<patch_SFXLooped> callback, bool obeysMasterPitch)
    {
        Name = name;
        sfxLoopCallback = callback;
        ObeysMasterPitch = obeysMasterPitch;
    }

    private patch_SFXLooped? cache;
    private patch_SFXLooped? GetActualSFXLooped()
    {
        if (cache != null)
        {
            return cache;
        }

        if (sfxLoopCallback != null)
        {
            return cache = sfxLoopCallback();
        }

        using var stream = Path!.Stream;
        return cache = new patch_SFXLooped(stream, ObeysMasterPitch);
    }
}
