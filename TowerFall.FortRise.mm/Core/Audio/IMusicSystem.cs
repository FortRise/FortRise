using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace FortRise;

public interface IMusicSystem 
{
    void Play(string name);
    void Resume();
    void Pause();
    void Stop(AudioStopOptions options);

    void Add(string name, Stream stream);
}