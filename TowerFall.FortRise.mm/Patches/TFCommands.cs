using Monocle;
using System.IO;
using System;

namespace TowerFall;

public static partial class patch_TFCommands 
{
    public extern static void orig_Init();

    public static void Init() 
    {
        orig_Init();
        Commands commands = Engine.Instance.Commands;
        commands.RegisterCommand("treasure_rates", args => 
        {
            if (Engine.Instance.Scene is not patch_Level level) 
            {
				commands.Log("Command can only be used during gameplay!");
                return;
            }
            if (!level.CanSpawnTreasure)
            {
				commands.Log("Treasure rates cannot be known if the level cannot spawn a treasure");
                return;
            }

            level.Session.TreasureSpawner.LogSpawnRates();
        });
        commands.RegisterCommand("hitbox", args => 
        {
            if (Engine.Instance.Scene is not Level) 
            {
				commands.Log("Command can only be used during gameplay!");
                return;
            }
            patch_Level.DebugMode = !patch_Level.DebugMode;
        });
        commands.RegisterCommand("summon", args => 
        {
            if (Engine.Instance.Scene is not patch_Level level) 
            {
				commands.Log("Command can only be used during gameplay!");
                return;
            }
            var portals = level.Layers[0].GetList<QuestSpawnPortal>();
            if (portals.Count == 0) 
            {
                commands.Log("No available portal found in this level.");
                return;
            }
            portals.Shuffle<QuestSpawnPortal>(); 
            portals[0].AppearAndSpawn(args[0]);
        });
        commands.RegisterCommand("dump_oel", args => 
        {
            if (Engine.Instance.Scene is not patch_Level level) 
            {
				commands.Log("Command can only be used during gameplay!");
                return;
            }
            string name = "dumplevel.oel";
            if (args.Length > 0)
                name = args[0] + ".oel";

            string dumpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DUMP");

            if (!Directory.Exists(dumpDir))
            {
                Directory.CreateDirectory(dumpDir);
            }
            
            string path = Path.Combine(dumpDir, name);
            level.XML.OwnerDocument.Save(path);
        });
        commands.RegisterCommand("logtags", args => 
        {
            Engine.Instance.Scene.LogTags();
        });
    }
}