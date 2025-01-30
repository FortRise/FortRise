using FortRise;
using MonoMod;

namespace Monocle;

public class patch_Entity : Entity
{
    [MonoModReplace]
    public void RemoveSelf() 
    {
        if (Scene != null)
        {
            Scene.Remove(this);
        }
        else 
        {
            Logger.Error("Entity being removed without a scene.");
        }
    }
}