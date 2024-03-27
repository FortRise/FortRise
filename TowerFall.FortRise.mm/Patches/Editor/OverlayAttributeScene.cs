using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall.Editor;

public class OverlayAttributeScene : EditorBase 
{
    public EditorScene Editor { get; private set; }
    public string ModeName => "ENTITY DATA";
    public List<Entity> ModeUI { get; private set; }

    public OverlayAttributeScene(EditorScene editor, patch_ActorData data) 
    {
        Editor = editor;
        SetLayer(0, new Monocle.Layer());
        ModeUI = new List<Entity>();
        InitActorAttributes(data);
    }

    public override void Begin()
    {
        // base.Add<OverlayBG>(new OverlayBG(this));
        base.Add<OverlayExit>(new OverlayExit(new Vector2(480, 620f)));
        base.Begin();
        Sounds.ed_overlayOn.Play(160f, 1f);
    }

    public override void End()
    {
        base.End();
        Sounds.ed_overlayOff.Play(160f, 1f);
    }

    public override void Update()
    {
        if (this.FocusedTextBox == null && (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Enter) || MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Space)))
        {
            Exit();
        }
        if (EditorBase.Ctrl && MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.S))
        {
            Editor.Save();
        }
        Editor.UpdateAutosave();
        base.Update();
    }

    public void Exit()
    {
        Engine.Instance.Scene = Editor;
        Editor.ReturnFromOverlay();
    }

    public override void Render()
    {
        Editor.Render();
        base.Render();
    }

    public override void HandleGraphicsReset()
    {
        base.HandleGraphicsReset();
        Editor.HandleGraphicsReset();
    }

    public override void HandleGraphicsDispose()
    {
        base.HandleGraphicsDispose();
        Editor.HandleGraphicsDispose();
    }

    private void InitActorAttributes(patch_ActorData data) 
    {
        foreach (var customData in data.CustomData) 
        {
            var key = customData.Key.ToUpperInvariant();
            var defaultValue = customData.Value.ToUpperInvariant();
            var textBox = new OverlayTextBox(new Vector2(480, 270f), key, defaultValue, 100, s => {
                data.CustomData[key] = s;
            });
            ModeUI.Add(textBox);
        }

        Add(ModeUI);
    }
}