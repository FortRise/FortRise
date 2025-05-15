using System;

namespace FortRise;

public struct TrackInfo 
{
    public string Name;
    public IResourceInfo Resource;

    public TrackInfo(string name, IResourceInfo resource) 
    {
        Name = name;
        Resource = resource;
    }

    public AudioTrack Create() 
    {
        var stream = Resource.Stream;
        if (Resource.ResourceType == typeof(RiseCore.ResourceTypeOggFile)) 
        {
            return new OggAudioTrack(stream);
        }
        if (Resource.ResourceType == typeof(RiseCore.ResourceTypeWavFile)) 
        {
            return new WavAudioTrack(stream);
        }

        Logger.Error("[TrackInfo] Unsupported Extension");
        return null;
    }
}