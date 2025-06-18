#nullable enable
using Monocle;

namespace FortRise;

internal sealed class SFXLoopedEntry : ISFXLoopedEntry
{
    public string Name { get; init; }
    public IResourceInfo Path { get; init; }
    public bool ObeysMasterPitch { get; init; }

    public patch_SFXLooped? SFXLooped => GetActualSFXLooped();

    public SFX? BaseSFX => SFXLooped;

    public SFXLoopedEntry(string name, IResourceInfo path, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
    }

    private patch_SFXLooped? cache;
    private patch_SFXLooped? GetActualSFXLooped()
    {
        if (cache != null)
        {
            return cache;
        }

        using var stream = Path.Stream;
        return cache = new patch_SFXLooped(stream, ObeysMasterPitch);
    }
}
