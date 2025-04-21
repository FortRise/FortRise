using FortRise;

namespace TowerFall;

public class patch_ArrowList : ArrowList
{
    private extern bool orig_IsLowPriorityArrow(ArrowTypes arrow);

    private bool IsLowPriorityArrow(ArrowTypes arrow)
    {
        return orig_IsLowPriorityArrow(arrow) || ArrowsRegistry.LowPriorityTypes.Contains(arrow);
    }

    private int GetFirstLowPriority()
    {
        ArrowTypes arrowTypes = ArrowTypes.Normal;
        if (Arrows.Contains(ArrowTypes.Toy))
        {
            arrowTypes = ArrowTypes.Toy;
        }
        for (int i = 0; i < Arrows.Count; i++)
        {
            var arrow = Arrows[i];
            if (arrow == arrowTypes || ArrowsRegistry.LowPriorityTypes.Contains(arrow))
            {
                return i;
            }
        }
        return -1;
    }
}