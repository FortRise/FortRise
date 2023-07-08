using System.Xml;
using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall.Editor;

public class patch_ActorSelector : ActorSelector
{
    public patch_ActorSelector(Vector2 position, ActorData data) : base(position, data)
    {
    }


    public extern void orig_OnMouseLeave();

    public override void OnMouseLeave()
    {
        if (Editor == null)
            return;
        orig_OnMouseLeave();
    }
}