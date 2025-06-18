#nullable enable
namespace FortRise;

internal sealed class CharacterSoundEntry : ICharacterSoundEntry
{
    public string Name { get; init; }
    public int SFXID { get; init; }
    public CharacterSoundConfiguration Configuration { get; init; }

    public CharacterSoundEntry(string name, int sfxID, CharacterSoundConfiguration configuration)
    {
        Name = name;
        SFXID = sfxID;
        Configuration = configuration;
    }
}
