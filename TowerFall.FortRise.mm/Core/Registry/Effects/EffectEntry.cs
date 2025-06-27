#nullable enable
namespace FortRise;

internal sealed class EffectEntry : IEffectEntry
{
    public string ID { get; init; }
    public EffectConfiguration Configuration { get; init; }

    public EffectResource? EffectResource
    {
        get
        {
            EffectManager.Shaders.TryGetValue(ID, out var effect);
            return effect;
        }
    }

    public EffectEntry(string id, EffectConfiguration configuration)
    {
        ID = id;
        Configuration = configuration;
    }
}
