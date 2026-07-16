using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall.Editor;

public class patch_NodeActorAgent : NodeActorAgent
{
    private patch_Actor actor;
    private Vector2 oldNode;
    private Vector2 newNode;

    public patch_NodeActorAgent(Actor actor, Vector2 moveTo) : base(actor, moveTo)
    {
    }

    [MonoModLinkTo("TowerFall.Editor.Agent", "System.Void .ctor()")]
    public void base_ctor() {}

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(patch_Actor actor, Vector2 moveTo)
    {
        base_ctor();
        this.actor = actor;
        newNode = actor.EnforceBounds(moveTo);
        if (actor.Nodes.Count > 0)
        {
            oldNode = actor.Nodes[0];
        }
    }

    [MonoModReplace]
    public void UpdateChange(Vector2 moveTo)
    {
        actor.Node = newNode = actor.EnforceBounds(moveTo);
        actor.Nodes[0] = newNode;
    }

    [MonoModReplace]
    public override void Do()
    {
        actor.Node = newNode;
        if (actor.Nodes.Count > 0)
        {
            actor.Nodes[0] = newNode;
        }
        else 
        {
            actor.Nodes.Add(newNode);
        }
    }

    [MonoModReplace]
    public override void Undo()
    {
        actor.Node = oldNode;
        if (actor.Nodes.Count > 0)
        {
            actor.Nodes[0] = oldNode;
        }
        else 
        {
            actor.Nodes.Add(oldNode);
        }
    }
}

