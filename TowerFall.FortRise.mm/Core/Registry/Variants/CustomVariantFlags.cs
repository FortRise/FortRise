using System;

namespace FortRise;

/// <summary>
/// A list of flags for variants behavior on the variant list.
/// </summary>
[Flags]
public enum CustomVariantFlags
{
    None = 0,
    PerPlayer = 1 << 0,
    CanRandom = 1 << 1,
    ScrollEffect = 1 << 2,
    CoopBlessing = 1 << 3,
    CoopCurses = 1 << 4,
    TournamentRule1v1 = 1 << 5,
    TournamentRule2v2 = 1 << 6,
    DarkWorldDLC = 1 << 7,
    Hidden = 1 << 8,
    Unlisted = 1 << 9,
}