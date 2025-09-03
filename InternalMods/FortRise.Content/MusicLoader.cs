using System.Collections.Generic;
using System.IO;

namespace FortRise.Content;

internal static class MusicLoader
{
    internal static void Load(IModRegistry registry, IModContent content, IFortRiseContentApi.ILoaderAPI.ILoader? loader)
    {
        loader ??= new Loader() { Path = ["Content/Music/*.ogg"] };

        if (loader.Path is null || !loader.Enabled)
        {
            return;
        }

        List<IResourceInfo> resources = [];
        
        foreach (var path in loader.Path)
        {
            resources.AddRange(content.Root.EnumerateChildrens(path));
        }

        foreach (var music in resources)
        {
            string name = Path.GetFileNameWithoutExtension(music.Name);
            registry.Musics.RegisterMusic(name, music);
        }
    }
}
