using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_OverlayScene : OverlayScene
{
    public EditorScene Editor
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        private set
        {
            throw new NotImplementedException();
        }
    }

    public List<Entity> ModeUI
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        private set
        {
            throw new NotImplementedException();
        }
    }

    public string ModeName
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModPublic]
        [MonoModIgnore]
        set
        {
            throw new NotImplementedException();
        }
    }

    private float oldMusicVolume;
    private bool noSetMode;

    public patch_OverlayScene(EditorScene editor) : base(editor) {}

    public patch_OverlayScene(EditorScene editor, bool setMode) : base(editor) {}

    [MonoModLinkTo("TowerFall.Editor.EditorBase", "System.Void .ctor()")]
    [MonoModIgnore]
    public void base_ctor() {}

    [MonoModConstructor]
    public void ctor(EditorScene editor, bool setMode)
    {
        base_ctor();
        Editor = editor;
        SetLayer(0, new Monocle.Layer());
        oldMusicVolume = Music.MasterVolume;
        Music.MasterVolume *= 0.65f;
        ModeUI = [];
        noSetMode = !setMode;
        if (setMode)
        {
            SetMode(Mode, true);
        }
        else 
        {
            ModeUI.Clear();
        }
    }

    [MonoModLinkTo("TowerFall.Editor.EditorBase", "System.Void Begin()")]
    [MonoModIgnore]
    public void base_Begin() {}

    [MonoModReplace]
    public override void Begin()
    {
        Add(new OverlayBG(this));
        Add(new OverlayExit(new Vector2(120f, 620f)));
        Add(new SaveGem());
        if (!noSetMode)
        {
            int num = Calc.EnumLength(typeof(Modes));
            for (int i = 0; i < num; i++)
            {
                Add(new OverlayModeButton(new Vector2(120f, 140 + 40 * i), (Modes)i));
            }
        }

        base_Begin();
        Sounds.ed_overlayOn.Play(160f, 1f);
    }
}
