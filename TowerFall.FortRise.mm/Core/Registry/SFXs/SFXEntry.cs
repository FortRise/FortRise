#nullable enable
using Monocle;

namespace FortRise;

internal sealed class SFXEntry : ISFXEntry
{
    public string Name { get; init; }
    public IResourceInfo Path { get; init; }
    public bool ObeysMasterPitch { get; init; }
    public SFX? SFX => GetActualSFX();

    public SFX? BaseSFX => SFX;


    public SFXEntry(string name, IResourceInfo path, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
    }

    private SFX? cache;
    private SFX? GetActualSFX()
    {
        if (cache != null)
        {
            return cache;
        }

        using var stream = Path.Stream;
        return cache = new patch_SFX(stream, ObeysMasterPitch);
    }
}
