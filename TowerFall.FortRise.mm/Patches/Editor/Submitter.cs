using MonoMod;
using TowerFall.Editor;

namespace TowerFall.Editor;

// Some users might have a itch, Humble or GOG version of the game.
[MonoModIfFlag("Steamworks")]
public class patch_Submitter : Submitter
{
    public patch_Submitter(EditorSubmit scene) : base(scene)
    {
    }

    [MonoModConstructor]
    [MonoModIgnore]
    [PatchSDL2ToSDL3]
    public extern void ctor(EditorSubmit scene);
}