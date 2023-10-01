using System;
using Monocle;
using TowerFall;

namespace FortRise;

internal abstract class GameMode 
{
    /// <summary>
    /// A session for this gamemode.
    /// </summary>
    public Session Session;

    /// <summary>
    /// A tower data used for this gamemode.
    /// </summary>
    public LevelData TowerData;

    /// <summary>
    /// A property used to instantiate the RoundLogic. Use the one that you defined or use `new LastManStandingRoundLogic(Session)` as default.
    /// </summary>
    public abstract RoundLogic RoundLogic { get; }

    /// <summary>
    /// A property used to instantiate the level system. Use the one that you defined or use `new VersusLevelSystem(TowerData as VersusTowerData)` as default.
    /// </summary> 
    public abstract LevelSystem LevelSystem { get; }

    public GameModeInfo Info;

    public GameMode(Session session, LevelData towerData) 
    {
        Session = session;
        TowerData = towerData;
    }

    /// <summary>
    /// This is where you initialize your gamemode to specify the name, icon, and the mode.
    /// </summary>
    public abstract GameModeInfo Initialize(GameModeBuilder builder);


    public enum GameModeType { Versus, CoOp }
}

internal struct GameModeInfo 
{
    public string Name;
    public Subtexture Icon;
    public GameMode.GameModeType GameMode;

    public GameModeInfo(string name, Subtexture icon, GameMode.GameModeType gameMode) 
    {
        Name = name;
        Icon = icon;
        GameMode = gameMode;
    }
}


/// <summary>
/// An attribute used to register your own custom game mode.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CustomGameModeAttribute : Attribute 
{
    public string Name;

    public CustomGameModeAttribute(string name) 
    {
        Name = name;
    }
}

/// <summary>
/// A class where you can initialize your GameMode accordingly. Use `Initialize` after you are done.
/// </summary>
internal class GameModeBuilder 
{
    public string GameModeName;
    public GameMode.GameModeType Type;
    public Subtexture GameModeIcon;

    public GameModeBuilder Name(string name) 
    {
        GameModeName = name;
        return this;
    }

    public GameModeBuilder Icon(Subtexture texture) 
    {
        GameModeIcon = texture;
        return this;
    }

    public GameModeBuilder Versus() 
    {
        Type = GameMode.GameModeType.Versus;
        return this;
    }

    public GameModeBuilder CoOp() 
    {
        Type = GameMode.GameModeType.CoOp;
        return this;
    }

    public GameModeInfo Build() 
    {
        return new GameModeInfo(GameModeName, GameModeIcon, Type);
    }
}