using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_ArrowTypePickup : ArrowTypePickup
{
    private GraphicsComponent graphic;
    public patch_ArrowTypePickup(Vector2 position, Vector2 targetPosition, ArrowTypes type) : base(position, targetPosition, type)
    {
    }

    public extern void orig_ctor(Vector2 position, Vector2 targetPosition, ArrowTypes type);

    [MonoModConstructor]
    public void ctor(Vector2 position, Vector2 targetPosition, ArrowTypes type)
    {
        orig_ctor(position, targetPosition, type);
        if (type <= ArrowTypes.Prism)
            return;
        if (!FortRise.RiseCore.PickupGraphicArrows.TryGetValue(type, out var info))
            return;
        
        if (info.Animated != null)
        {
            graphic = info.Animated;
            Add(graphic);
            return;
        }
        if (info.Simple != null)
        {
            graphic = info.Simple;
            Add(graphic);
        }
    }
}