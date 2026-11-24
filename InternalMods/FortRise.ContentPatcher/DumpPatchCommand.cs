using System;
using System.IO;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise.ContentPatcher;

internal static class DumpPatchCommand
{
    public static void Register(IModRegistry registry, LevelPatcher levelPatcher)
    {
        registry.Commands.RegisterCommands("contentpatcher_dump_patch", new CommandConfiguration
        {
            Callback = args => Run(levelPatcher, args)
        });
    }

    private static void Run(LevelPatcher levelPatcher, string[] args)
    {
        var commands = Engine.Instance.Commands;
        if (Engine.Instance.Scene is not Level level)
        {
            commands.Log("Command can only be used during gameplay!");
            return;
        }

        if (levelPatcher.LastPatched is not { } patched || !ReferenceEquals(patched.Patched, level.XML))
        {
            commands.Log("The current level is not patched by ContentPatcher.");
            return;
        }

        string name = args.Length > 0 ? args[0] : "patchedlevel";
        string dumpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DUMP", "ContentPatcher");
        Directory.CreateDirectory(dumpDir);

        string patchedPath = Path.Combine(dumpDir, name + ".oel");
        string originalPath = Path.Combine(dumpDir, name + ".original.oel");
        Save(patched.Patched, patchedPath);
        Save(patched.Original, originalPath);

        commands.Log($"Level: {patched.Tower} ({patched.LevelPath ?? "unknown"})");
        commands.Log($"Applied patches: {string.Join(", ", patched.AppliedPatches)}");
        commands.Log($"Dumped to {patchedPath}");
        commands.Log($"Original dumped to {originalPath}");
    }

    private static void Save(XmlElement level, string path)
    {
        var document = new XmlDocument();
        document.AppendChild(document.ImportNode(level, true));
        document.Save(path);
    }
}
