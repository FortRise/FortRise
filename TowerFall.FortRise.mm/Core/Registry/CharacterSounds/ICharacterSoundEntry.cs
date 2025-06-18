#nullable enable
namespace FortRise;

public interface ICharacterSoundEntry
{
    public string Name { get; init; }
    public int SFXID { get; init; }
    public CharacterSoundConfiguration Configuration { get; init; }
}
