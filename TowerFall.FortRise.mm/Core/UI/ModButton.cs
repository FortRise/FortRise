using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using TowerFall;

namespace FortRise;

public sealed class ModButton : OptionsButton
{
    private DynamicData data;
    private Wiggler data_changedWiggler => data.Get<Wiggler>("changedWiggler");
    private Wiggler data_selectedWiggler => data.Get<Wiggler>("selectedWiggler");
    private int data_wiggleDir => data.Get<int>("wiggleDir");
    private string cachedTitle;

    public ModButton(string title) : base(title)
    {
        data = DynamicData.For(this);
        cachedTitle = title;
    }

    public override void Render()
    {
        Vector2 vector = new Vector2(30f + 2f * data_changedWiggler.Value * (float)data_wiggleDir, 0f);
        Color color = (base.Selected ? OptionsButton.SelectedColor : OptionsButton.NotSelectedColor);
        Draw.OutlineTextCentered(TFGame.Font, cachedTitle, this.Position + new Vector2(-5f, 0f) + new Vector2(5f * data_selectedWiggler.Value, 0f), color, Color.Black, 1f);
        base_Render();
    }

    public override void Removed()
    {
        base.Removed();
        data.Dispose();
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
    [MonoModIgnore]
    public void base_Render() 
    {
        base.Render();
    }
}