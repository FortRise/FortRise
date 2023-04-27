#pragma warning disable CS0626
#pragma warning disable CS0108
using MonoMod;

namespace TowerFall;

public class patch_Session : Session
{
    public patch_Session(MatchSettings settings) : base(settings)
    {
    }

    [MonoModIgnore]
    [PatchSessionStartGame]
    public extern void StartGame();
}