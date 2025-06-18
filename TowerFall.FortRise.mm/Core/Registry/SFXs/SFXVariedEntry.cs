#nullable enable
using System.IO;
using Monocle;

namespace FortRise;

internal sealed class SFXVariedEntry : ISFXVariedEntry
{
    public string Name { get; init; }
    public IResourceInfo[] Variations { get; init; }
    public bool ObeysMasterPitch { get; init; }

    public patch_SFXVaried? SFXVaried => GetActualSFXVaried();

    public int Count { get; init; }

    public SFX? BaseSFX => SFXVaried;

    public SFXVariedEntry(string name, IResourceInfo[] variations, int count, bool obeysMasterPitch)
    {
        Name = name;
        Variations = variations;
        ObeysMasterPitch = obeysMasterPitch;
        Count = count;
    }

    private patch_SFXVaried? cache;
    private patch_SFXVaried? GetActualSFXVaried()
    {
        if (cache != null)
        {
            return cache;
        }

        Stream[] stream = new Stream[Count];
        try
        {
            for (int i = 0; i < Count; i++)
            {
                stream[i] = Variations[i].Stream;
            }

            return cache = new patch_SFXVaried(stream, Count, ObeysMasterPitch);
        }
        finally
        {
            for (int i = 0; i < Count; i++)
            {
                stream[i].Dispose();
            }
        }
    }
}
