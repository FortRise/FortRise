using FortRise;
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
        if (!RiseCore.ArrowNameMap.TryGetValue(type, out var id)) 
            return;
        
        var info = RiseCore.ArrowsRegistry[id].InfoLoader?.Invoke();
        if (info == null)
            return;
        var value = info.Value;
        
        if (value.Animated != null)
        {
            graphic = value.Animated;
            Add(graphic);
            return;
        }
        if (value.Simple != null)
        {
            graphic = value.Simple;
            Add(graphic);
        }
    }
}