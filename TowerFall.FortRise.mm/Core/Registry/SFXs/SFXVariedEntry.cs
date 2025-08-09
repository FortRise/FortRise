#nullable enable
using System;
using System.IO;
using Monocle;

namespace FortRise;

internal sealed class SFXVariedEntry : ISFXVariedEntry
{
    public string Name { get; init; }
    public IResourceInfo[] Variations { get; init; }
    public bool ObeysMasterPitch { get; init; }

    public patch_SFXVaried? SFXVaried => GetActualSFXVaried();

    public int Count { get => count; init => count = value; }

    public SFX? BaseSFX => SFXVaried;
    private int count;
    private Func<patch_SFXVaried>? sfxVariedCallback;

    public SFXVariedEntry(string name, IResourceInfo[] variations, int count, bool obeysMasterPitch)
    {
        Name = name;
        Variations = variations;
        ObeysMasterPitch = obeysMasterPitch;
        Count = count;
    }

    public SFXVariedEntry(string name, Func<patch_SFXVaried> callback, bool obeysMasterPitch)
    {
        Name = name;
        Variations = Array.Empty<IResourceInfo>();
        ObeysMasterPitch = obeysMasterPitch;
        sfxVariedCallback = callback;
    }

    private patch_SFXVaried? cache;
    private patch_SFXVaried? GetActualSFXVaried()
    {
        if (cache != null)
        {
            return cache;
        }

        if (sfxVariedCallback != null)
        {
            cache = sfxVariedCallback();
            count = cache.Datas.Length;
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
