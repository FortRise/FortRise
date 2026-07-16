using Microsoft.Xna.Framework;

namespace TowerFall.Editor;

public class patch_PlayButton : PlayButton
{
    public patch_PlayButton(Vector2 position) : base(position)
    {
    }

    public extern void orig_OnMouseClick(Vector2 localPosition);

    public override void OnMouseClick(Vector2 localPosition)
    {
        var actors = Editor.CurrentLevel.ActorsLayer.Actors;
        bool invalid = false;
        foreach (patch_Actor actor in actors)
        {
            if (actor.Invalid)
            {
                invalid = true;
                break;
            }
        }

        if (invalid)
        {
            Sounds.ui_invalid.Play();
            return;
        }

        orig_OnMouseClick(localPosition);
    }
}
