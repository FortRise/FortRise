using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public abstract class patch_Arrow : Actor
{
    public int? OverrideCharacterIndex;
    public int? OverridePlayerIndex;
    private static Stack<patch_Arrow>[] cached;
    public abstract ArrowTypes ArrowType { [MonoModReplace] get; set; }

    public static int ARROW_TYPES;

    public static string[] Names;
    public static Color[] Colors;
    public static Color[] ColorsB;
    public static Color[] NoneColors;

    protected patch_Arrow() : base(Vector2.Zero)
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    public static void cctor() 
    {
        ARROW_TYPES = Calc.EnumLength(typeof(ArrowTypes));
        Names = new string[]
        {
            "+2",
            "BOMB",
            "SUPER BOMB",
            "LASER",
            "BRAMBLE",
            "DRILL",
            "BOLT",
            "TOY",
            "FEATHER",
            "TRIGGER",
            "PRISM"
        };
        Colors = new Color[]
        {
            Calc.HexToColor("F7EAC3"),
            Calc.HexToColor("F8B800"),
            Calc.HexToColor("F8B800"),
            Calc.HexToColor("B8F818"),
            Calc.HexToColor("F87858"),
            Calc.HexToColor("8EE8FF"),
            Calc.HexToColor("00FF4C"),
            Calc.HexToColor("FF6DFA"),
            Calc.HexToColor("BC70FF"),
            Calc.HexToColor("1BB7EE"),
            Calc.HexToColor("DB4ADB")
        };
        ColorsB = new Color[]
        {
            Calc.HexToColor("FFFFFF"),
            Calc.HexToColor("F7D883"),
            Calc.HexToColor("F7D883"),
            Calc.HexToColor("D0F76C"),
            Calc.HexToColor("F7B09E"),
            Calc.HexToColor("D8F7FF"),
            Calc.HexToColor("00D33B"),
            Calc.HexToColor("FFB5FC"),
            Calc.HexToColor("D5A5FF"),
            Calc.HexToColor("56D4FF"),
            Calc.HexToColor("FF52FF")
        };
        NoneColors = new Color[]
        {
            Calc.HexToColor("F83800"),
            Calc.HexToColor("F87858")
        }; 
        var arrowIDCount = ARROW_TYPES + FortRise.RiseCore.ArrowsID.Count;
        Array.Resize(ref Names, arrowIDCount);
        Array.Resize(ref Colors, arrowIDCount);
        Array.Resize(ref ColorsB, arrowIDCount);
        foreach (var arrow in RiseCore.ArrowsID.Values) 
        {
            var loader = RiseCore.PickupGraphicArrows[arrow];
            var info = loader?.Invoke();
            if (info == null)
                return;
            var value = info.Value;
            Names[(int)arrow] = value.Name.ToUpperInvariant();
            Colors[(int)arrow] = Calc.HexToColor(value.Color);
            ColorsB[(int)arrow] = Calc.HexToColor(value.ColorB);
        }
    }

    public override void Added()
    {
        base.Added();
    }

    [MonoModReplace]
    public static void Initialize() 
    {
        cached = new Stack<patch_Arrow>[Arrow.ARROW_TYPES + FortRise.RiseCore.ArrowsID.Count];
        for (int i = 0; i < cached.Length; i++)
        {
            cached[i] = new Stack<patch_Arrow>();
        }
    }

    [MonoModReplace]
    public static patch_Arrow Create(ArrowTypes type, LevelEntity owner, Vector2 position, float direction, int? overrideCharacterIndex = null, int? overridePlayerIndex = null)
    {
        patch_Arrow arrow;
        if (cached[(int)type].Count > 0)
        {
            arrow = cached[(int)type].Pop();
        }
        else
        {
            arrow = type switch 
            {
                ArrowTypes.Normal => new patch_DefaultArrow(),
                ArrowTypes.Bomb => new patch_BombArrow(),
                ArrowTypes.SuperBomb => new patch_SuperBombArrow(),
                ArrowTypes.Laser => new patch_LaserArrow(),
                ArrowTypes.Bramble => new patch_BrambleArrow(),
                ArrowTypes.Drill => new patch_DrillArrow(),
                ArrowTypes.Bolt => new patch_BoltArrow(),
                ArrowTypes.Toy => new patch_ToyArrow(),
                ArrowTypes.Feather => new patch_FeatherArrow(),
                ArrowTypes.Trigger => new patch_TriggerArrow(),
                ArrowTypes.Prism => new patch_PrismArrow(),
                _ => CreateCustomArrow(type)
            };
        }
        arrow.OverrideCharacterIndex = overrideCharacterIndex;
        arrow.OverridePlayerIndex = overridePlayerIndex;
        arrow.Init(owner, position, direction);
        return arrow;
    }

    private static patch_Arrow CreateCustomArrow(ArrowTypes type) 
    {
        var arrow = RiseCore.Arrows[type]?.Invoke();
        if (arrow == null) 
        {
            Logger.Error($"Some Arrow type ID: {type} can't be found. Falling back to default arrow");
            return new patch_DefaultArrow();
        }
        return arrow;
    }

    [MonoModIgnore]
    protected extern virtual void Init(LevelEntity owner, Vector2 position, float direction);
}

public class patch_DefaultArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Normal;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Normal;
    }
}

public class patch_BombArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Bomb;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Bomb;
    }
}

public class patch_SuperBombArrow : patch_Arrow
{
    private ArrowTypes arrowTypes;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.SuperBomb;
    }
}

public class patch_LaserArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Laser;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Laser;
    }
}

public class patch_BrambleArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Bramble;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Bramble;
    }
}

public class patch_DrillArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Drill;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Drill;
    }
}

public class patch_BoltArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Bolt;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Bolt;
    }
}

public class patch_ToyArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Toy;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Toy;
    }
}

public class patch_FeatherArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Feather;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Feather;
    }
}

public class patch_TriggerArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Trigger;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Trigger;
    }
}

public class patch_PrismArrow : patch_Arrow
{
    private ArrowTypes arrowTypes = ArrowTypes.Prism;
    public override ArrowTypes ArrowType { get => arrowTypes; set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Prism;
    }
}