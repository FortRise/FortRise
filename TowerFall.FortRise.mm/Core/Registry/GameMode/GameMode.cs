using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

/// <summary>
/// A class to create custom gamemode for Versus or Co-Op (WIP) mode. By extending this class, 
/// the mod loader will then proceed to register the class into the GameModeRegistry.
/// </summary>
public abstract class CustomGameMode 
{
    /// <summary>
    /// The ID of your GameMode, this can only be set by the mod loader.
    /// </summary>
    public string ID { get; internal set; }
    internal TowerFall.Modes GameModeInternal;

    /// <summary>
    /// The name that would show up on the gamemode selection, by default
    /// it will use the type name instead.
    /// </summary>
    public string Name 
    { 
        get => name ?? StringUtils.SeparateCases(GetType().Name); 
        set => name = value; 
    }
    private string name;

    /// <summary>
    /// The color of the name that would show up in the round start screen.
    /// </summary>
    public Color NameColor { get => nameColor; set => nameColor = value; }
    private Color nameColor = Color.White;

    /// <summary>
    /// A huge icon that shows up on the gamemode selection.
    /// </summary>
    public Subtexture Icon 
    { 
        get => icon; 
        set => icon = value; 
    }
    private Subtexture icon = TFGame.MenuAtlas["gameModes/lastManStanding"];

    /// <summary>
    /// A type whether it should be Versus or Co-Op mode.
    /// </summary>
    public GameModeType ModeType { get => type; set => type = value; }
    private GameModeType type;

    /// <summary>
    /// A flag to switch the mode to team mode.
    /// </summary>
    public bool TeamMode { get => teamMode; set => teamMode = value; }
    private bool teamMode;

    /// <summary>
    /// A minimum players to play for team mode.
    /// </summary>
    public int TeamMinimumPlayers { get; set; } = 3;

    /// <summary>
    /// A minimum players to play for FFA mode.
    /// </summary>
    public int MinimumPlayers { get; set; } = 2;

    /// <summary>
    /// The level that has a flag fixedFirst will never have an effect and will random the first level if this flag
    /// is set to false.
    /// </summary>
    public bool RespectFixedFirst { get; set; }

    /// <summary>
    /// An X-Offset of a coin show up on the result.
    /// </summary>
    public int CoinOffset { get => coinOffset; set => coinOffset = value; }
    private int coinOffset = 10;

    /// <summary>
    /// A sound for a coin earned on the result.
    /// </summary>
    public SFX EarnedCoinSound { get => earnedCoinSound; set => earnedCoinSound = value; }
    private SFX earnedCoinSound = Sounds.sfx_multiCoinEarned;

    /// <summary>
    /// A sound for a coin lost on the result.
    /// </summary>
    public SFX LoseCoinSound { get => loseCoinSound; set => loseCoinSound = value; }
    private SFX loseCoinSound = Sounds.sfx_multiSkullNegative;
    internal Sprite<int> coinSprite;

    internal void InitializeSoundsInternal() 
    {
        earnedCoinSound = Sounds.sfx_multiCoinEarned;
        loseCoinSound = Sounds.sfx_multiSkullNegative;
        InitializeSounds();
    }

    /// <summary>
    /// Creates a custom game mode, this must be registered to the GameModeRegistry.
    /// </summary>
    public CustomGameMode() 
    {
        ID = GetType().Name;
    }

    /// <summary>
    /// Initialize the fields you need to initialize such as Name, Icon and ModeType. 
    /// <br/>
    /// For initializing SFX, <see cref="FortRise.CustomGameMode.InitializeSounds"/>.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Initialize your SFX here such as <see cref="FortRise.CustomGameMode.EarnedCoinSound"/> 
    /// and <see cref="FortRise.CustomGameMode.LoseCoinSound"/>.
    /// </summary>
    public abstract void InitializeSounds();

    /// <summary>
    /// Initialize your game mode state here
    /// <param name="session">A game session that you can access if you need to modify some parts of it</param>
    /// </summary>
    public virtual void StartGame(Session session) {}

    /// <summary>
    /// Instantiate your round logic here to create your own logic for your gamemode.
    /// You can either use the vanilla round logic or derived atleast one of it with its different behaviour.
    /// <param name="session">A game session needed to be passed for round logic</param>
    /// </summary>
    public abstract RoundLogic CreateRoundLogic(Session session);
    
    /// <summary>
    /// Instatiate your own level system here to create a custom level system.
    /// <param name="levelData">A data needed to be passed for level systems</param>
    /// </summary>
    public virtual LevelSystem GetLevelSystem(LevelData levelData) 
    {
        if (ModeType == GameModeType.Versus) 
        {
            return new VersusLevelSystem(levelData as VersusTowerData);
        }
        return new QuestLevelSystem(levelData as QuestLevelData);
    }

    /// <summary>
    /// A sprite to used for points system. Default is the coin (Last Man Standing or TDM).
    /// <br/>
    /// To change it to skull, override this function and return UseSkullSprite()
    /// </summary>
    public virtual Sprite<int> CoinSprite() 
    {
        return VersusCoinButton.GetCoinSprite();
    }

    /// <summary>
    /// A ready to use skull sprite with offset adjusted as well.
    /// </summary>
    public Sprite<int> UseSkullSprite() 
    {
        CoinOffset = 12;
        return DeathSkull.GetSprite();
    }
}

/// <summary>
/// An enum type used to specify where to put this game mode.
/// </summary>
public enum GameModeType { Versus, CoOp }