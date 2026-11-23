using System.Collections.Generic;

namespace Monocle;

public class patch_Layer : Layer
{
    internal static List<Entity> Recorder;

    internal extern void orig_Add(Entity entity, bool layerChange = false);

    internal void Add(Entity entity, bool layerChange = false)
    {
        orig_Add(entity, layerChange);
        if (!layerChange)
        {
            Recorder?.Add(entity);
        }
    }
}
