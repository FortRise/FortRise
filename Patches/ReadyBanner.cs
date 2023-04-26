#pragma warning disable CS0626
#pragma warning disable CS0108

using MonoMod;

namespace TowerFall;

public class patch_ReadyBanner : ReadyBanner 
{
    [PatchGetReadyState]
    [MonoModIgnore]
    private extern bool GetReadyState();
}