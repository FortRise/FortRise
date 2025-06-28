using System.IO;

namespace FortRise.Content;

internal static class MusicLoader
{
    internal static void Load(IModRegistry registry, IModContent content)
    {
        if (!content.Root.TryGetRelativePath("Content/Music", out IResourceInfo musicRes))
        {
            return;
        }

        foreach (var music in musicRes.Childrens)
        {
            string name = Path.GetFileNameWithoutExtension(music.Name);
            registry.Musics.RegisterMusic(name, music);
        }
    }
}