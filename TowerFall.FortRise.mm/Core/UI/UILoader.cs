using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;
 
public class UILoader : Loader
{
    public bool Finished;

    public UILoader() : base(true)
    {
    }
 
    public void Finish()
    {
        Finished = true;
        RemoveSelf();
    }
 
    public override void Render()
    {
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.5f);
        base.Render();
        Draw.OutlineTextCentered(TFGame.Font, "LOADING", this.Position + new Vector2(0f, 22f), Color.White, Color.Black);
    }
}