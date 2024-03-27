using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_ActorSelector : ActorSelector
{
    private patch_ActorData data;
    private List<OverlayTextBox> textBoxes;
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

    public override void OnMouseRightClick(Vector2 localPosition)
    {
        if (data.CustomData.Count == 0)
            return;

        Engine.Instance.Scene = new OverlayAttributeScene(base.Scene as EditorScene, data);

    }
}