using Microsoft.Xna.Framework;

namespace TowerFall.Editor;

public class NodeExtendedActorAgent : Agent
{
    private patch_Actor actor;
    private Vector2 newNode;
    private int nodeIndex;

    public NodeExtendedActorAgent(patch_Actor actor, Vector2 moveTo) 
    {
        this.actor = actor;
        newNode = actor.EnforceBounds(moveTo);
    }

    public void UpdateChange(Vector2 moveTo)
    {
        newNode = actor.EnforceBounds(moveTo);
        actor.Nodes[nodeIndex] = newNode;
    }

    public override void Do()
    {
        nodeIndex = actor.Nodes.Count;
        actor.Nodes.Add(newNode);
    }

    public override void Undo()
    {
        actor.Nodes.RemoveAt(nodeIndex);
    }
}


