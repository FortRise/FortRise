using Microsoft.Xna.Framework;
using MonoMod;

namespace TowerFall.Editor;

public class NodeExtendedRemoveActorAgent : Agent
{
    private patch_Actor actor;
    private Vector2 node;

    public NodeExtendedRemoveActorAgent(patch_Actor actor) 
    {
        this.actor = actor;
    }

    [MonoModReplace]
    public override void Do()
    {
        node = actor.Nodes[^1];
        actor.Nodes.Remove(node);
    }

    [MonoModReplace]
    public override void Undo()
    {
        actor.Nodes.Add(node);
    }

}



