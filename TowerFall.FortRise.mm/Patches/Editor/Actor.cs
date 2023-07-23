using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TowerFall.Editor;

public class patch_Actor : Actor
{
    public Dictionary<string, patch_ActorData> Data;

    public patch_Actor(Level level, Vector2 position, ActorData data) : base(level, position, data)
    {
    }
}