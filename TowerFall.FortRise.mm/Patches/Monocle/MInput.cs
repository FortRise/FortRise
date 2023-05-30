using MonoMod;

namespace Monocle;

public static class patch_MInput 
{
    [MonoModIgnore]
    internal static extern void Initialize();
}