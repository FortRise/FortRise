using System;

namespace FortRise;

/// <summary>
/// A list of flags for variants behavior on the variant list.
/// </summary>
[Flags]
public enum CustomVariantFlags
{
    None,
    PerPlayer,
    CanRandom,
    ScrollEffect,
    CoopBlessing,
    CoopCurses,
    TournamentRule1v1,
    TournamentRule2v2,
    DarkWorldDLC,
    Hidden,
    Unlisted,
}