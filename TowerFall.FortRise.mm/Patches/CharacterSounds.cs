using FortRise.IO;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_CharacterSounds : CharacterSounds
{
    public patch_CharacterSounds(string prefix, CharacterSounds original = null) : base(prefix, original)
    {
    }

    [MonoModReplace]
    private bool Exists(string name)
    {
        return ModIO.IsFileExists(Audio.LOAD_PREFIX + Prefix + name + ".wav");
    }
}