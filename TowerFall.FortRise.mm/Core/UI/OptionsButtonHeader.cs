using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

// we inherit this to OptionsButton so that we can still effectively put this in a collection
// but we of course remove some logic
public class OptionsButtonHeader : OptionsButton
{
    private string title;
    public OptionsButtonHeader(string title) : base(title)
    {
        this.title = title;
    }

    [MonoModLinkTo("TowerFall.MenuItem", "Update")]
    public void base_Update() { }

    [MonoModLinkTo("TowerFall.MenuItem", "Render")]
    public void base_Render() { }

    public override void Update()
    {
        base_Update();
    }

    public override void Render()
    {
        Draw.OutlineTextJustify(
            TFGame.Font, title,
            Position,
            Color.White, Color.Black, new Vector2(1f, 0.5f), 1.0f);
        base_Render();
    }
}