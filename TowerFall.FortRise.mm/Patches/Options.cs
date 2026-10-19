using System;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.Options")]
public class Options : TowerFall.Options
{
    [MonoModReplace]
    [MonoModConstructor]
    public void ctor()
    {
        Fullscreen = true;
        MusicVolume = 0.8f;
        SFXVolume = 0.8f;

        GamepadVibration = true;
        GamepadDiscovery = true;
        AllowDirectInput = true;

        HoldToPause = false;
        ShowTips = true;
        ReplayMode = ReplayModes.UseGPU;

        CanSkipReplays = true;
    }

    [MonoModReplace]
    public void AfterLoad()
    {
        if (Fullscreen || Environment.GetEnvironmentVariable("SteamDeck") == "1")
        {
            Engine.Instance.Screen.EnableFullscreen(Screen.FullscreenMode.LargestScale);
        }
        else
        {
            Engine.Instance.Screen.DisableFullscreen();
        }

        Audio.MasterVolume = SFXVolume;
        Music.MasterVolume = MusicVolume;
        MInput.GamepadVibration = GamepadVibration;

        if (!VerticalSync)
        {
            Engine.Instance.Graphics.SynchronizeWithVerticalRetrace = VerticalSync;
            Engine.Instance.Graphics.ApplyChanges();
        }

        Coroutine.OneFrameDelay = !VerticalSync;
    }

}
