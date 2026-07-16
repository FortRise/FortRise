using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_ActorSelector : ActorSelector
{
    private ActorData data;
    private Wiggler rotateWiggler;


    public patch_ActorSelector(Vector2 position, ActorData data) : base(position, data)
    {
    }

    [MonoModReplace]
    public override void OnMouseLeave()
    {
        Depth++;
        if (Editor is null)
        {
            return;
        }

        if (Editor.ActorBrush != data)
        {
            rotateWiggler.Start();
        }
    }
}
