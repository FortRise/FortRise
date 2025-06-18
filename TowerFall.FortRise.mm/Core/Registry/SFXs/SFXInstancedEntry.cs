#nullable enable
using Monocle;

namespace FortRise;

internal sealed class SFXInstancedEntry : ISFXInstancedEntry
{
    public string Name { get; init; }
    public IResourceInfo Path { get; init; }
    public bool ObeysMasterPitch { get; init; }
    public int Instances { get; init; }

    public patch_SFXInstanced? SFXInstanced => GetActualSFXInstanced();

    public SFX? BaseSFX => SFXInstanced;

    public SFXInstancedEntry(string name, IResourceInfo path, int instances, bool obeysMasterPitch)
    {
        Name = name;
        Path = path;
        ObeysMasterPitch = obeysMasterPitch;
        Instances = instances;
    }

    private patch_SFXInstanced? cache;
    private patch_SFXInstanced? GetActualSFXInstanced()
    {
        if (cache != null)
        {
            return cache;
        }

        using var stream = Path.Stream;
        return cache = new patch_SFXInstanced(stream, Instances, ObeysMasterPitch);
    }
}
