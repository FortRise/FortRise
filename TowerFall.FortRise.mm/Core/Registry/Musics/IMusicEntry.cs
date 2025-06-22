#nullable enable
namespace FortRise;

public interface IMusicEntry
{
    public string Name { get; init; }
    public IResourceInfo MusicPath { get; init; }
}
