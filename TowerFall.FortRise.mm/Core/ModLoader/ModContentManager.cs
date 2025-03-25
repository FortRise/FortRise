using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace FortRise;

// we need a way to use the full directory of TowerFall.
// we may add additional feature here in the future.
public class ModContentManager : ContentManager
{
    public ModContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
    {
    }

    protected override Stream OpenStream(string assetName)
    {
        string path = Path.GetFullPath(Path.Combine(RootDirectory, "Content", assetName) + ".xnb");
        if (File.Exists(path))
        {
            return File.OpenRead(path);
        }
        return base.OpenStream(assetName);
    }
}