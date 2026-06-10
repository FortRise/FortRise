using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.Variant")]
public class Variant : TowerFall.Variant
{
    internal bool IsCustom;

    public Variant(Subtexture icon, string title, string description, Pickups[] itemExclusions, bool perPlayer, string header, UnlockData.Unlocks? unlocker, bool scrollEffect, bool hidden, bool canRandom, bool tournamentRule1v1, bool tournamentRule2v2, bool unlisted, bool darkWorldDLC, int coOpValue) : base(icon, title, description, itemExclusions, perPlayer, header, unlocker, scrollEffect, hidden, canRandom, tournamentRule1v1, tournamentRule2v2, unlisted, darkWorldDLC, coOpValue)
    {
    }
}