using FortRise;
using Monocle;

namespace TowerFall;

public partial class patch_TFCommands 
{
    public static bool CheatMode;

    public static partial void CheatMethod(Commands command) 
    {
        command.RegisterCommand("cheats", args => 
        {
            CheatMode = !CheatMode;
        });
    }
}