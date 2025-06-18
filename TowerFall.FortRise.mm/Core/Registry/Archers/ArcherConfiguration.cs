#nullable enable
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public readonly struct ArcherConfiguration()
{
    public required string TopName { get; init; }
    public required string BottomName { get; init; }
    public required ISubtextureEntry Aimer { get; init; }
    public required ICorpseSpriteContainerEntry CorpseSprite { get; init; }

    public IArcherEntry? AltFor { get; init; }
    public IArcherEntry? SecretFor { get; init; }

    public Option<HairInfo> Hair { get; init; }
    public int SFX { get; init; }

    public Color ColorA { get; init; } = Color.White;
    public Color ColorB { get; init; } = Color.White;
    public Color LightbarColor { get; init; } = Color.White;
    public bool StartNoHat { get; init; }
    public string? VictoryMusic { get; init; }
    public bool PurpleParticles { get; init; }
    public TFGame.Genders Gender { get; init; }
    public Option<HatInfo> Hat { get; init; }
    public required SpriteInfo Sprites { get; init; }
    public required PortraitInfo Portraits { get; init; }
    public required StatueInfo Statue { get; init; }
    public required GemInfo Gems { get; init; }
    public Option<ArcherData.BreathingInfo> Breathing { get; init; }
}

public readonly struct GemInfo
{
    public required IMenuSpriteContainerEntry Menu { get; init; }
    public required ISpriteContainerEntry Gameplay { get; init; }
}

public readonly struct SpriteInfo
{
    public required ISpriteContainerEntry Body { get; init; }
    public required ISpriteContainerEntry HeadNormal { get; init; }
    public required ISpriteContainerEntry HeadNoHat { get; init; }
    public required ISpriteContainerEntry HeadCrown { get; init; }
    public required ISpriteContainerEntry Bow { get; init; }
    public ISpriteContainerEntry? HeadBack { get; init; }
}