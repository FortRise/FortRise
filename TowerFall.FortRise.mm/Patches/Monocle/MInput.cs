using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoMod;
using static Monocle.MInput;

namespace Monocle;

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
        Joysticks = new List<JoystickData>();
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
                Calc.Log("Removed XGamepad: " + xGamepadData);
                MInput.GamepadsChanged = true;
                XGamepads[i].Dispose();
                // TODO stop rumble XGamepad
                MInput.XGamepads.RemoveAt(i);
                i--;
            }
        }
        if (MInput.XGamepads.Count >= 4)
            return;
        
        for (int i = MInput.XGamepads.Count; i < 4; i++)
        {
            if (GamePad.GetState((PlayerIndex)i).IsConnected)
            {
                MInput.XGamepads.Add(new MInput.XGamepadData((PlayerIndex)i));
                Calc.Log("Add XGamepad: " + i);
                MInput.GamepadsChanged = true;
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

    public class patch_XGamepadData
    {
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