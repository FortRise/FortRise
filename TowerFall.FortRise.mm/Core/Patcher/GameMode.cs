using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;


public abstract class CustomGameMode 
{
    public string ID { get; internal set; }
    internal TowerFall.Modes GameModeInternal;

    public string Name { get => name; set => name = value; }
    private string name = "Unknown";
    public Color NameColor { get => nameColor; set => nameColor = value; }
    private Color nameColor = Color.White;
    public Subtexture Icon { get => icon; set => icon = value; }
    private Subtexture icon = TFGame.MenuAtlas["gameModes/lastManStanding"];
    public GameModeType ModeType { get => type; set => type = value; }
    private GameModeType type;

    public bool TeamMode { get => teamMode; set => teamMode = value; }
    private bool teamMode;

    public int CoinOffset { get => coinOffset; set => coinOffset = value; }
    private int coinOffset = 10;

    public SFX EarnedCoinSound { get => earnedCoinSound; set => earnedCoinSound = value; }
    private SFX earnedCoinSound = Sounds.sfx_multiCoinEarned;

    public SFX LoseCoinSound { get => loseCoinSound; set => loseCoinSound = value; }
    private SFX loseCoinSound = Sounds.sfx_multiSkullNegative;
    internal Sprite<int> coinSprite;

    internal void InitializeSoundsInternal() 
    {
        earnedCoinSound = Sounds.sfx_multiCoinEarned;
        loseCoinSound = Sounds.sfx_multiSkullNegative;
        InitializeSounds();
    }

    public CustomGameMode() 
    {
        ID = GetType().Name;
    }

    public abstract void Initialize();
    public abstract void InitializeSounds();

    public abstract RoundLogic CreateRoundLogic(Session session);
    

    public virtual LevelSystem GetLevelSystem(LevelData levelData) 
    {
        if (ModeType == GameModeType.Versus) 
        {
            return new VersusLevelSystem(levelData as VersusTowerData);
        }
        return new QuestLevelSystem(levelData as QuestLevelData);
    }

    public virtual Sprite<int> CoinSprite() 
    {
        return VersusCoinButton.GetCoinSprite();
    }
}

public enum GameModeType { Versus, CoOp }