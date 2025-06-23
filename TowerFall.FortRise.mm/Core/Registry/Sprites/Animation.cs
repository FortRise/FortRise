#nullable enable
namespace FortRise;

public readonly struct Animation<T>
{
    public required T ID { get; init; }
    public required int[] Frames { get; init; }
    public float Delay { get; init; }
    public bool Loop { get; init; }
}
