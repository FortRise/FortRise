#nullable enable
namespace FortRise;

public interface IEffectEntry
{
    public string ID { get; init; }
    public EffectConfiguration Configuration { get; init; }
    public EffectResource? EffectResource { get; }
}
