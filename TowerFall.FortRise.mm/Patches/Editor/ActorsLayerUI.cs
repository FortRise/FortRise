using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_ActorsLayerUI : ActorsLayerUI
{
    private DrawMode? mode;
    private patch_Actor actor;
    private Vector2 actorClickOffset;
    private float sineCounter;

    private ActorsRemoveAgent removeAgent;
    private MoveActorAgent moveAgent;
    private ResizeActorAgent resizeAgent;
    private NodeActorAgent nodeAgent;
    private NodeExtendedActorAgent nodeExtendedActorAgent;
    private NodeExtendedRemoveActorAgent nodeExtendedRemoveActorAgent;


    public patch_Actor MousedActor
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

    public patch_ActorsLayerUI()
    {
    }

    [MonoModReplace]
    public override void OnMouseOver(Vector2 localPosition)
    {
        MousedActor = null;
        int i = Level.ActorsLayer.Actors.Count - 1;
        if (mode == null)
        {
            while (i >= 0)
            {
                patch_Actor.MouseMode mouseMode = 
                    (patch_Actor.MouseMode)Level.ActorsLayer.Actors[i].CheckPosition(localPosition);

                switch (mouseMode)
                {
                    case patch_Actor.MouseMode.Resize:
                        MousedActor = (patch_Actor)Level.ActorsLayer.Actors[i];
                        MousedActor.ResizeMoused = true;
                        goto EXIT;
                    case patch_Actor.MouseMode.Node:
                    case patch_Actor.MouseMode.NodeExtended:
                        MousedActor = (patch_Actor)Level.ActorsLayer.Actors[i];
                        MousedActor.NodeMoused = true;
                        goto EXIT;
                    case patch_Actor.MouseMode.Attribute:
                        MousedActor = (patch_Actor)Level.ActorsLayer.Actors[i];
                        MousedActor.AttributeMoused = true;
                        goto EXIT;
                }

                Level.ActorsLayer.Actors[i].ResizeMoused = false;
                Level.ActorsLayer.Actors[i].NodeMoused = false;
                (Level.ActorsLayer.Actors[i] as patch_Actor).AttributeMoused = false;
                if (mouseMode == patch_Actor.MouseMode.Main)
                {
                    MousedActor = (patch_Actor)Level.ActorsLayer.Actors[i];
                    break;
                }

                i--;
            }

            EXIT: {}
        }
        else
        {
            MousedActor = actor;
            if (mode.Value == DrawMode.Resizing)
            {
                MousedActor.ResizeMoused = true;
            }
            else if (mode.Value == DrawMode.Node)
            {
                MousedActor.NodeMoused = true;
            }
            else if (mode.Value == DrawMode.NodeExtended)
            {
                MousedActor.NodeMoused = true;
            }
            else if (mode.Value == DrawMode.Attribute)
            {
                MousedActor.AttributeMoused = true;
            }
        }
        for (i--; i >= 0; i--)
        {
            Level.ActorsLayer.Actors[i].ResizeMoused = false;
            Level.ActorsLayer.Actors[i].NodeMoused = false;
            (Level.ActorsLayer.Actors[i] as patch_Actor).AttributeMoused = false;
        }
    }

    [MonoModReplace]
    public override void OnMouseRightClick(Vector2 localPosition)
    {
        mode = DrawMode.Deleting;
        removeAgent = new ActorsRemoveAgent();
        Actor.MouseMode shimMouseMode = Actor.MouseMode.None;
        patch_Actor actorAt = (patch_Actor)Editor.CurrentLevel.ActorsLayer.GetActorAt(localPosition, ref shimMouseMode);

        patch_Actor.MouseMode mouseMode = (patch_Actor.MouseMode)shimMouseMode;
        if (actorAt != null)
        {
            if (mouseMode == patch_Actor.MouseMode.NodeExtended)
            {
                nodeExtendedRemoveActorAgent = new NodeExtendedRemoveActorAgent(actorAt);
                Level.AgentStack.Do(nodeExtendedRemoveActorAgent);
                return;
            }

            if (removeAgent.ChangeCount == 0)
            {
                Level.AgentStack.Do(removeAgent);
            }
            removeAgent.AddChange(actorAt);
            Sounds.ed_actorErase.Play(160f, 1f);
        }
    }

    [MonoModReplace]
    public override void OnMouseClick(Vector2 localPosition)
    {
        Actor.MouseMode shimMouseMode = Actor.MouseMode.None;
        patch_Actor actorAt = (patch_Actor)Level.ActorsLayer.GetActorAt(localPosition, ref shimMouseMode);

        patch_Actor.MouseMode mouseMode = (patch_Actor.MouseMode)shimMouseMode;
        if (actorAt != null)
        {
            if (EditorBase.Shift)
            {
                if (Level.ActorsLayer.CanAddActors)
                {
                    actor = new patch_Actor(actorAt);

                    Level.AgentStack.Do(new ActorAddAgent(actor));
                    actorClickOffset = actorAt.Position - localPosition;
                    mode = DrawMode.MovingCreated;
                    Sounds.ed_actorPlace.Play(160f, 1f);
                }
                else
                {
                    EditorBase.SFXActorPlaceFail();
                }
            }

            else if (EditorBase.Ctrl)
            {
                if (Level.ActorsLayer.CanAddActors)
                {
                    Vector2 position = actorAt.Position;
                    position.X = 160f + (160f - actorAt.Position.X);
                    if (actorAt.CanSymmetryClone && !Editor.CurrentLevel.ActorsLayer.AlreadyExists(actorAt.Data, position))
                    {
                        actor = new patch_Actor(actorAt)
                        {
                            Position = position
                        };
                        if (actor.Data.HasNode)
                        {
                            actor.Node.X = 160f + (160f - actor.Node.X);
                        }
                        Level.AgentStack.Do(new ActorAddAgent(actor));
                        mode = null;
                        Sounds.ed_actorPlace.Play(160f, 1f);
                    }
                }
                else
                {
                    EditorBase.SFXActorPlaceFail();
                }
            }
            else if (mouseMode == patch_Actor.MouseMode.Resize)
            {
                mode = DrawMode.Resizing;
                actorClickOffset = localPosition;
                actor = actorAt;
            }
            else if (mouseMode == patch_Actor.MouseMode.Node)
            {
                mode = DrawMode.Node;
                actorClickOffset = actorAt.Position - localPosition;
                actor = actorAt;
            }
            else if (mouseMode == patch_Actor.MouseMode.Attribute)
            {
                mode = DrawMode.Attribute;
                actorClickOffset = actorAt.Position - localPosition;
                actor = actorAt;
            }
            else if (mouseMode == patch_Actor.MouseMode.NodeExtended)
            {
                mode = DrawMode.NodeExtended;
                actorClickOffset = actorAt.Nodes[^1] - localPosition;
                actor = actorAt;
            }
            else
            {
                mode = DrawMode.Moving;
                actorClickOffset = actorAt.Position - localPosition;
                actor = actorAt;
            }
        }

        else if (Editor.ActorBrush != null)
        {
            if (Level.ActorsLayer.CanAddActors)
            {
                Vector2 vector = LocalToGrid(localPosition, 5f);
                actor = new patch_Actor(Level, vector, Editor.ActorBrush);
                Level.AgentStack.Do(new ActorAddAgent(actor));
                mode = DrawMode.MovingCreated;
                Sounds.ed_actorPlace.Play(160f, 1f);
            }
            else
            {
                EditorBase.SFXActorPlaceFail();
            }
        }
    }


    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() {}

    public override void Update()
    {
        base_Update();
        sineCounter = (sineCounter + 0.31415927f * Engine.TimeMult) % 6.2831855f;
        foreach (Actor actor in Level.ActorsLayer.Actors)
        {
            actor.Update(this);
        }

        Vector2 relMousePos = MInput.Mouse.Position - Position;
        relMousePos /= 2f;

        if (mode == null)
        {
            return;
        }

        switch (mode.Value)
        {
            case DrawMode.Deleting:
                {
                    Actor actorAt = Editor.CurrentLevel.ActorsLayer.GetActorAt(relMousePos);
                    if (actorAt != null)
                    {
                        if (removeAgent.ChangeCount == 0)
                        {
                            Level.AgentStack.Do(removeAgent);
                        }
                        removeAgent.AddChange(actorAt);
                    }

                    break;
                }

            case DrawMode.MovingCreated:
                {
                    Vector2 placementPos = LocalToGrid(relMousePos, 5f);
                    actor.Position = actor.EnforceBounds(placementPos);
                    actor.OnCreateMove();
                    break;
                }

            case DrawMode.Moving:
                {
                    Vector2 placementPos = LocalToGrid(relMousePos + actorClickOffset, 5f);
                    if (actor.Position != placementPos)
                    {
                        if (moveAgent == null)
                        {
                            moveAgent = new MoveActorAgent(actor, placementPos);
                            Level.AgentStack.Do(moveAgent);
                        }
                        else
                        {
                            moveAgent.UpdateChange(placementPos);
                        }
                    }

                    break;
                }

            case DrawMode.Resizing:
                {
                    Vector2 resizeDirection = relMousePos - actorClickOffset;
                    int horizontal = (int)Math.Round((double)(resizeDirection.X / 10f)) * 10;
                    int vertical = (int)Math.Round((double)(resizeDirection.Y / 10f)) * 10;
                    if (!actor.Data.ResizeableX)
                    {
                        horizontal = 0;
                    }

                    if (!actor.Data.ResizeableY)
                    {
                        vertical = 0;
                    }

                    if (horizontal != 0 || vertical != 0)
                    {
                        if (horizontal == 0)
                        {
                            horizontal = -1;
                        }
                        else
                        {
                            horizontal = (int)MathHelper.Clamp(actor.Width + horizontal, actor.Data.MinWidth, actor.Data.MaxWidth);
                            if (horizontal == actor.Width)
                            {
                                horizontal = -1;
                            }
                        }

                        if (vertical == 0)
                        {
                            vertical = -1;
                        }
                        else
                        {
                            vertical = (int)MathHelper.Clamp(actor.Height + vertical, actor.Data.MinHeight, actor.Data.MaxHeight);
                            if (vertical == actor.Height)
                            {
                                vertical = -1;
                            }
                        }

                        if (horizontal != -1)
                        {
                            actorClickOffset.X += horizontal - actor.Width;
                        }
                        if (vertical != -1)
                        {
                            actorClickOffset.Y += vertical - actor.Height;
                        }

                        int width = actor.Width;
                        int height = actor.Height;
                        if (resizeAgent == null)
                        {
                            resizeAgent = new ResizeActorAgent(actor, horizontal, vertical);
                            Level.AgentStack.Do(resizeAgent);
                        }
                        else
                        {
                            resizeAgent.UpdateChange(horizontal, vertical);
                        }

                        if (width != actor.Width || height != actor.Height)
                        {
                            EditorBase.SFXTick();
                        }
                    }

                    break;
                }

            case DrawMode.Node:
                {
                    Vector2 placementPos = LocalToGrid(relMousePos + actorClickOffset, 5f) + new Vector2(actor.Width / 2, actor.Height / -2);
                    if (actor.Node != placementPos)
                    {
                        if (nodeAgent == null)
                        {
                            nodeAgent = new NodeActorAgent(actor, placementPos);
                            Level.AgentStack.Do(nodeAgent);
                        }
                        else
                        {
                            nodeAgent.UpdateChange(placementPos);
                        }
                    }

                    break;
                }

            case DrawMode.NodeExtended:
                {
                    Vector2 placementPos = LocalToGrid(
                        relMousePos + actorClickOffset, 5f) + 
                        new Vector2(actor.Width / 2, actor.Height / -2);

                    if (actor.Node != placementPos)
                    {
                        if (nodeExtendedActorAgent == null)
                        {
                            nodeExtendedActorAgent = new NodeExtendedActorAgent(actor, placementPos);
                            Level.AgentStack.Do(nodeExtendedActorAgent);
                        }
                        else
                        {
                            nodeExtendedActorAgent.UpdateChange(placementPos);
                        }
                    }
                }
                break;

            case DrawMode.Attribute:
                // make sure to not run again..
                // there could be a better way though?
                if (actor is not null)
                {
                    Engine.Instance.Scene = new OverlayAttributeScene(Scene as EditorScene, actor);
                    actor = null;
                }
                break;
        }
    }

    public extern void orig_OnUnClick();

    public override void OnUnClick()
    {
        orig_OnUnClick();
        if (mode is null)
        {
            return;
        }

        if (mode.Value == DrawMode.Attribute)
        {
            mode = null;
            actor = null;
        }
        else if (mode.Value == DrawMode.NodeExtended)
        {
            mode = null;
            nodeExtendedActorAgent = null;
        }
    }

    public enum DrawMode
    {
        Deleting,
        MovingCreated,
        Moving,
        Resizing,
        Node,
        NodeExtended,
        Attribute
    }
}
