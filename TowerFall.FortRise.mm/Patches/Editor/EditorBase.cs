using System;
using System.IO;
using MonoMod;

namespace TowerFall.Editor;

public class patch_EditorBase : EditorBase
{
    public static string WorkshopDirectory
    {
        [MonoModReplace]
        get
        {
            return Path.Combine(Environment.CurrentDirectory, "Workshop");
        }
    }
}