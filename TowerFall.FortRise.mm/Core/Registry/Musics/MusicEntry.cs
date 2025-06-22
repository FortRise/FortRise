#nullable enable
namespace FortRise;

internal sealed class MusicEntry : IMusicEntry
{
    public string Name { get; init; }
    public IResourceInfo MusicPath { get; init; }

    public MusicEntry(string name, IResourceInfo musicPath)
    {
        Name = name;
        MusicPath = musicPath;
    }
}
