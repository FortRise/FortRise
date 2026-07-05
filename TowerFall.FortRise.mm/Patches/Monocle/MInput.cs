using System;
using System.Collections.Generic;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoMod;
using SDL3;

namespace Monocle;

[MonoModPatch("MInput")]
[MonoModIfFlag("OS:Windows")]
public static class DInputRemoval 
{
    [MonoModRemove]
    private static object DirectInput;
    [MonoModRemove]
    public static List<MInput.JoystickData> Joysticks;

    [MonoModReplace]
    public static string[] LogJoysticks()
    {
        return Array.Empty<string>();
    }

    [MonoModPatch("JoystickData")]
    [MonoModRemove]
    public class JoystickData {}
} 

public static class patch_MInput 
{
    public static patch_KeyboardData Keyboard;
    public static patch_MouseData Mouse;
    public static List<patch_XGamepadData> XGamepads;

    [MonoModReplace]
    internal static void Initialize() 
    {
        Keyboard = new patch_KeyboardData();
        Mouse = new patch_MouseData();
        XGamepads = new List<patch_XGamepadData>();
        UpdateJoysticks();
        foreach (var gamepad in MInput.XGamepads) 
        {
            gamepad.StopRumble();
        }
    }

    [MonoModReplace]
    internal static void Update() 
    {
        if (Engine.Instance.IsActive) 
        {
            if (Engine.Instance.Commands.Open)
                Keyboard.UpdateNull();
            else
                Keyboard.Update();
            
            Mouse.Update();
            if (MInput.UpdateXInput) 
            {
                foreach (var gamepadData in XGamepads) 
                {
                    gamepadData.Update(true);
                }
            }
        }
        else 
        {
            Keyboard.UpdateNull();
            if (MInput.UpdateXInput) 
            {
                foreach (var gamepad in XGamepads) 
                {
                    gamepad.Update(false);
                }
            }
        }
        MInput.GamepadsChanged = false;
    }

    [MonoModReplace]
    internal static void UpdateJoysticks()
    {
        for (int i = 0; i < XGamepads.Count; i++)
        {
            var xGamepadData = XGamepads[i];
            if (!xGamepadData.Attached)
            {
                Logger.Info("Removed XGamepad: " + xGamepadData);
                MInput.GamepadsChanged = true;
                XGamepads[i].Dispose();
                MInput.XGamepads.RemoveAt(i);
                i--;
            }
        }


        if (MInput.XGamepads.Count < 4)
        {
            for (int i = 0; i < 4; i++)
            {
                if (GamePad.GetState((PlayerIndex)i).IsConnected)
                {
                    var device = FrameworkPlatform.GetGamepadDevice(i);
                    var id = SDL.SDL_GetGamepadID(device);

                    foreach (var gamepad in XGamepads)
                    {
                        var instanceID = gamepad.InstanceID;
                        if (id == instanceID)
                        {
                            goto SKIP;
                        }
                    }

                    var gamepadData = new patch_XGamepadData((PlayerIndex)i, id);
                    XGamepads.Add(gamepadData);
                    Logger.Info("Add XGamepad: " + gamepadData);
                    MInput.GamepadsChanged = true;

                    SKIP: {}

                    if (XGamepads.Count >= 4)
                    {
                        break;
                    }
                }
            }
        }
    }

    [MonoModReplace]
    internal static void Shutdown() 
    {
        foreach (var gamepad in MInput.XGamepads) 
        {
            gamepad.StopRumble();
        }
        Keyboard.Dispose();
    }

    public class patch_XGamepadData : MInput.XGamepadData
    {
        [MonoModPublic]
        public PlayerIndex PlayerIndex;
        public uint InstanceID;
        private Counter rumbleCounter;

        public patch_XGamepadData(PlayerIndex playerIndex) : base(playerIndex)
        {
        }

        public patch_XGamepadData(PlayerIndex playerIndex, uint id) : base(playerIndex)
        {
        }

        [MonoModConstructor]
        public void ctor(PlayerIndex playerIndex, uint id)
        {
            PlayerIndex = playerIndex;
            InstanceID = id;
            rumbleCounter = new Counter();
        }

        public bool Attached
        {
            [MonoModIgnore]
            get => false;
        }
        [MonoModIgnore]
        internal extern void Update(bool focus);

        [MonoModIgnore]
        internal extern void Dispose();
    }

    public class patch_MouseData 
    {
        [MonoModIgnore]
        internal extern void UpdateNull();

        [MonoModIgnore]
        internal extern void Update();

        [MonoModIgnore]
        internal extern void Dispose();
    }

    public class patch_KeyboardData 
    {
        [MonoModIgnore]
        internal extern void UpdateNull();

        [MonoModIgnore]
        internal extern void Update();

        [MonoModIgnore]
        internal extern void Dispose();
    }
}

internal static class FrameworkPlatform
{
    public static IntPtr GetGamepadDevice(int index)
    {
        var platform = typeof(Vector2).Assembly.GetType("Microsoft.Xna.Framework.SDL3_FNAPlatform");
        var devices = platform.GetField("INTERNAL_devices", BindingFlags.Static | BindingFlags.NonPublic);
        return ((IntPtr[])devices.GetValue(null))[index];
    }
}