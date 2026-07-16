using System;
using FortRise.Forms;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_EditorScene : EditorScene
{
    public Tower Tower
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

    private TowerOverlayButton overlayButton;
    private ActorSelector lanternSelector;
    private bool hasBegun;
    private bool refreshed;
    public int OnActorLayer { get; set; }
    public patch_EditorScene(Tower tower) : base(tower)
    {
    }

    public extern void orig_Begin();

    public override void Begin()
    {
        bool hasBegunBefore = hasBegun;
        orig_Begin();
        // we rebuild without refreshing the actor
        if (ActorsLayerUI.Collidable || !hasBegunBefore)
        {
            RebuildActorSelector(false, hasBegunBefore);
        }
    }


    public void RebuildActorSelector(bool refresh, bool hasBegun = true) 
    {
        foreach (var actorLayer in Layers[0].Entities) 
        {
            if (actorLayer is ActorSelector or ActorLeftButton or ActorRightButton)
            {
                Remove(actorLayer);
            }
        }

        int maxLength = Math.Min((OnActorLayer * 36) + 36, patch_ActorData.ActorDatas.Count);

        var i = 0;
        foreach (var value in patch_ActorData.ActorDatas[(OnActorLayer * 36)..maxLength])
        {
            ActorSelector actorSelector = new ActorSelector(
                LevelUIPosition + new Vector2(50 + i % 12 * 50, 515 + i / 12 * 50), value)
            {
                Visible = hasBegun,
                Collidable = hasBegun 
            };
            Add(actorSelector);

            i += 1;
            if (value.Name == "BGLantern")
            {
                lanternSelector = actorSelector;
            }
        }

        refreshed = refresh;

        var actorLeftButton = new ActorLeftButton(
            LevelUIPosition + new Vector2(10, 515 + 1 * 50),
            this
        )
        {
            Visible = hasBegun 
        };

        Add(actorLeftButton);

        var actorRightButton = new ActorRightButton(
            LevelUIPosition + new Vector2(45 + 12 * 50, 515 + 1 * 50),
            this
        )
        {
            Visible = hasBegun
        };

        Add(actorRightButton);
    }

    public extern void orig_Update();

    public override void Update()
    {
        if (refreshed) 
        {
            refreshed = false;
            SetActiveLayer(ActorsLayerUI);
        }
        orig_Update();
    }

    [MonoModReplace]
    public void Open()
    {
        IgnoreHotkeysFrame = true;

        var open = new OpenFileDialog() 
        {
            InitialDirectory = WorkshopDirectory,
            Title = "Load a .tower file."
        };

        if (open.ShowDialog() == DialogResult.Success)
        {
            Tower = new Tower(Calc.LoadXML(open.FileName))
            {
                LastSavedFilename = open.FileName
            };
            SetLevel(Tower.Levels[0], false);
            RefreshCustomAssets();
            Background.Refresh();
            SolidsLayerUI.RefreshTileset();
            BGLayerUI.RefreshTileset();
            TilesLayerUI.RefreshTileset();
            TileSelector.RefreshTileset();
            RefreshLanternGraphics();
            overlayButton.Refresh();
        }
    }

    [MonoModReplace]
    public void RefreshLanternGraphics()
    {
        if (ActorData.Data.TryGetValue("BGLantern", out var value))
        {
            value.Subtexture = TFGame.EditorAtlas[$"actors/{Tower.Lanterns}"];
        }

        lanternSelector.RefreshImage();
    }
}
