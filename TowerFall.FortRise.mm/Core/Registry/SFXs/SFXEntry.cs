#nullable enable
using System;
using Monocle;

namespace FortRise;

internal sealed class SFXEntry : ISFXEntry
{
    public string Name { get; init; }
    public IResourceInfo? Path { get; init; }
    public bool ObeysMasterPitch { get; init; }
    public SFX? SFX => GetActualSFX();

    public SFX? BaseSFX => SFX;
    private Func<SFX>? sfxCallback;


    public SFXEntry(string name, IResourceInfo path, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
    }

    public SFXEntry(string name, Func<SFX> sfx, bool obeysMasterPitch) 
    {
        Name = name;
        Path = null;
        sfxCallback = sfx;
        ObeysMasterPitch = obeysMasterPitch;
    }

    private SFX? cache;
    private SFX? GetActualSFX()
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
        return cache = new patch_SFX(stream, ObeysMasterPitch);
    }
}
