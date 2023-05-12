using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall;

public abstract class patch_Arrow 
{
    public int? OverrideCharacterIndex;
    public int? OverridePlayerIndex;
    private static Stack<patch_Arrow>[] cached;
    public abstract ArrowTypes ArrowType { [MonoModReplace] get; internal set; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

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
    public override ArrowTypes ArrowType { get => arrowTypes; internal set => arrowTypes = value; }

    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() 
    {
        orig_ctor();
        arrowTypes = ArrowTypes.Prism;
    }
}