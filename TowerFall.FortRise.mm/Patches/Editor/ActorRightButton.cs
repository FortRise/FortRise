using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace TowerFall.Editor;

public class ActorRightButton : EditorUI
{
    private Subtexture subA;
    private Image image;
    private Wiggler rotateWiggler;
    private float pressedCounter;

    public ActorRightButton(Vector2 position, patch_EditorScene scene) 
        : base(position, 40, 40, -20, -20)
    {
        Tag(GameTags.EditorActorPalette);
        if (scene.OnActorLayer > (ActorData.Data.Count / (12 * 3) - 1))
        {
            subA = TFGame.EditorAtlas["levels/rightNone"];
        }
        else
        {
            subA = TFGame.EditorAtlas["levels/right"];
        }

        image = new Image(subA, null);
        image.CenterOrigin();
        image.Color = Color.LightGray;
        image.Scale = Vector2.One * 2f;
        Add(image);

        rotateWiggler = Wiggler.Create(30, 5f, null, (v) =>
        {
            if (Hovered)
            {
                image.Rotation = v * -6f * 0.017453292f;
            }
            else
            {
                image.Rotation = v * -3f * 0.017453292f;
            }
        }, false, false);
        Add(rotateWiggler);
    }

    public override void Update()
    {
        base.Update();

        if (pressedCounter > 0f)
        {
            pressedCounter -= Engine.TimeMult;
            if (pressedCounter <= 0f)
            {
                image.SwapSubtexture(subA, null);
            }
        }
    }

    public override void OnHotkey(Keys key, bool repeating)
    {
        if (!repeating || Editor.NextLevel != null)
        {
            OnMouseClick(Vector2.Zero);
        }
    }

    public override void OnMouseEnter()
    {
        if (CanAct)
        {
            Sounds.ed_buttonMouse.Play(160f, 1f);
            rotateWiggler.Start();
            image.Color = Color.White;
        }
    }

    public override void OnMouseLeave()
    {
        if (CanAct)
        {
            rotateWiggler.Start();
            image.Color = Color.LightGray;
        }
    }

    public override void OnMouseClick(Vector2 localPosition)
    {
        if (CanAct)
        {
            Sounds.ed_arrowBrowse.Play(160f, 1f);
            (Editor as patch_EditorScene).OnActorLayer += 1;
            (Editor as patch_EditorScene).RebuildActorSelector(true);

            pressedCounter = 6f;
        }
    }

    public bool CanAct
    {
        get
        {
            return !(
            (Editor as patch_EditorScene)?.OnActorLayer > (ActorData.Data.Count / (12 * 3) - 1));
        }
    }
}

