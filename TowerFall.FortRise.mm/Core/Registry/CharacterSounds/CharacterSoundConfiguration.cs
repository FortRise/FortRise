#nullable enable
namespace FortRise;

public readonly struct CharacterSoundConfiguration
{
    public required ISFXVariedEntry Ready { get; init; }
    public required ISFXEntry Deselect { get; init; }
    public required ISFXEntry Aim { get; init; }
    public required ISFXEntry AimCancel { get; init; }
    public required ISFXEntry AimDir { get; init; }
    public required ISFXEntry Die { get; init; }
    public required ISFXEntry DieBomb { get; init; }
    public required ISFXEntry DieLaser { get; init; }
    public required ISFXEntry DieStomp { get; init; }
    public required ISFXEntry DieEnv { get; init; }
    public required ISFXEntry Duck { get; init; }
    public required ISFXEntry FireArrow { get; init; }
    public required ISFXEntry Grab { get; init; }
    public required ISFXEntry Jump { get; init; }
    public required ISFXEntry Land { get; init; }
    public required ISFXEntry NoFire { get; init; }
    public required ISFXLoopedEntry WallSlide { get; init; }
    public required ISFXEntry ArrowSteal { get; init; }
    public required ISFXEntry ArrowGrab { get; init; }
    public required ISFXEntry ArrowRecover { get; init; }
    public required ISFXEntry Revive { get; init; }
    public ISFXLoopedEntry? Sleep { get; init; }
}
