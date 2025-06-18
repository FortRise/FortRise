using Monocle;
using MonoMod;
using FortRise;

namespace TowerFall;

public class patch_CharacterSounds : CharacterSounds
{
    public patch_CharacterSounds() : base(null, null)
    {

    }
    public patch_CharacterSounds(string prefix, CharacterSounds original = null) : base(prefix, original)
    {
    }

    [MonoModConstructor]
    public void ctor() {}

    [MonoModReplace]
    private bool Exists(string name)
    {
        return ModIO.IsFileExists(Audio.LOAD_PREFIX + Prefix + name + ".wav");
    }
}