using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace TowerFall.Editor;

public class patch_EditorScene : EditorScene
{
    private ActorSelector lanternSelector;
    private bool hasBegun;
    private bool refreshed;
    public int onLayer;
    public patch_EditorScene(Tower tower) : base(tower)
    {
    }

    public extern void orig_Begin();

    public override void Begin()
    {
        if (!hasBegun)
        {
            RegisterHotkey(() => {
                if (onLayer <= 0)
                    return;
                Logger.Log("Back: " + onLayer);
                onLayer--;
                ActorData.Data = patch_ActorData.DataLayers[onLayer];
                RebuildActorSelector();
            }, Keys.OemOpenBrackets, true, false);
            RegisterHotkey(() => {
                Logger.Log(onLayer);
                if (onLayer > ActorData.Data.Count - 1)
                    return;
                Logger.Log("Finalized: " + onLayer);
                onLayer++;
                ActorData.Data = patch_ActorData.DataLayers[onLayer];
                RebuildActorSelector();
            }, Keys.OemCloseBrackets, true, false);
        }
        orig_Begin();
    }

    private void RebuildActorSelector() 
    {
        foreach (var actorLayer in Layers[0].Entities) 
        {
            if (actorLayer is ActorSelector)
            {
                Remove(actorLayer);
            }
        }
        var num13 = 0;
        foreach (KeyValuePair<string, ActorData> keyValuePair in ActorData.Data)
        {
            ActorSelector actorSelector = new ActorSelector(EditorScene.LevelUIPosition + new Vector2((float)(50 + num13 % 12 * 50), (float)(515 + num13 / 12 * 50)), keyValuePair.Value);
            base.Add<ActorSelector>(actorSelector);
            num13++;
            if (keyValuePair.Key == "BGLantern")
            {
                this.lanternSelector = actorSelector;
            }
        }
        refreshed = true;
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
}