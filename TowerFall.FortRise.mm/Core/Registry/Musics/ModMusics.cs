#nullable enable
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModMusics
{
    IMusicEntry? GetMusic(string id);
    IMusicEntry RegisterMusic(string id, IResourceInfo filePath);
}

internal sealed class ModMusics : IModMusics
{
    private readonly ModuleMetadata metadata;
    private readonly RegistryQueue<IMusicEntry> queue;

    internal ModMusics(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        queue = manager.CreateQueue<IMusicEntry>(metadata, Invoke, Detach);
    }

    public IMusicEntry RegisterMusic(string id, IResourceInfo filePath)
    {
        string name = $"{metadata.Name}/{id}";
        IndexReservationPool.For(metadata)?.TryClaim("music", name, out bool _);
        IMusicEntry entry = new MusicEntry(name, filePath);
        MusicRegistry.AddMusic(entry);
        queue.AddOrInvoke(entry);
        return entry;
    }

    public IMusicEntry? GetMusic(string id)
    {
        return MusicRegistry.GetMusic(id);
    }

    private void Invoke(IMusicEntry entry)
    {
        var trackInfo = new TrackInfo(entry.Name, entry.MusicPath);
        patch_Music.TrackMap[entry.Name] = trackInfo;
    }

    private void Detach(IMusicEntry entry)
    {
        patch_Music.TrackMap.Remove(entry.Name);
        MusicRegistry.RemoveMusic(entry);

        // a track registered again under the same name keeps playing through a hot reload
        if (IndexReservationPool.For(metadata) is { } session)
        {
            session.Reserve("music", entry.Name, true, () => StopTrack(entry.Name));
            return;
        }

        StopTrack(entry.Name);
    }

    private static void StopTrack(string name)
    {
        if (patch_Music.StopTrack(name) && Engine.Instance.Scene is MainMenu)
        {
            MainMenu.PlayMenuMusic();
        }
    }
}
