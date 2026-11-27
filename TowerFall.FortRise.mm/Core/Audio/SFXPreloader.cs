using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace FortRise;

internal static class SFXPreloader
{
    private static readonly ConcurrentDictionary<string, byte[]> sfxCaches = new();

    public static void Preload(string directory)
    {
        string dir = FileSystemUtils.NormalizedPath(directory);
        if (!Directory.Exists(dir))
        {
            return;
        }

        Parallel.ForEach(Directory.EnumerateFiles(dir, "*.wav"), (file) =>
        {
            sfxCaches[Path.GetFileName(file)] = File.ReadAllBytes(file);
        });
    }

    public static Stream OpenRead(string path)
    {
        if (ModIO.IsFileExists(path) && !File.Exists(FileSystemUtils.NormalizedPath(path)))
        {
            return ModIO.OpenRead(path);
        }

        if (sfxCaches.TryRemove(Path.GetFileName(path), out var bytes))
        {
            return new MemoryStream(bytes, writable: false);
        }

        return ModIO.OpenRead(path);
    }

    public static void Clear()
    {
        sfxCaches.Clear();
    }
}
