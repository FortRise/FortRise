using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

namespace TowerFall;

[Serializable]
public class GamepadConfig
{
    public string ButtonSet = "Automatic";

    public Buttons[] Left = [Buttons.DPadLeft];
    public Buttons[] Right = [Buttons.DPadRight];
    public Buttons[] Up = [Buttons.DPadUp];
    public Buttons[] Down = [Buttons.DPadDown];
    public Buttons[] Jump = [Buttons.A];
    public Buttons[] Shoot = [Buttons.X];
    public Buttons[] AltShoot = [Buttons.B];
    public Buttons[] Dodge = [Buttons.RightShoulder, Buttons.RightTrigger];
    public Buttons[] Arrows = [Buttons.Y];
    public Buttons[] MenuAlt = [Buttons.LeftShoulder, Buttons.LeftTrigger];
    public Buttons[] Start = [Buttons.Start];

    public float MoveXDeadzone = 0.5f;
    public float MoveYDeadzone = 0.8f;

    public static GamepadConfig GetDefault()
    {
        return new GamepadConfig();
    }

    public static GamepadConfig[] GetDefaults()
    {
        var configs = new GamepadConfig[4];
        for (int i = 0; i < configs.Length; i += 1)
        {
            configs[i] = GetDefault();
        }

        return configs;
    }

    public static Subtexture GetIcon(string buttonSet, Buttons button)
    {
        var map = Patching.XGamepadInput.ButtonIconMap[buttonSet];

        try
        {
            var startText = map.ChildText("jump");
            var length = startText.IndexOf('/');
            var text = startText[..length];

            return button switch
            {
                Buttons.A => TFGame.MenuAtlas["controls/" + map.ChildText("jump")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.B => TFGame.MenuAtlas["controls/" + map.ChildText("altShoot")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.X => TFGame.MenuAtlas["controls/" + map.ChildText("shoot")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Y => TFGame.MenuAtlas["controls/" + map.ChildText("arrows")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Start => TFGame.MenuAtlas["controls/" + map.ChildText("start")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.RightTrigger => TFGame.MenuAtlas["controls/" + map.ChildText("alt")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.RightShoulder => TFGame.MenuAtlas["controls/" + text + "/rb"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.LeftTrigger => TFGame.MenuAtlas["controls/" + map.ChildText("alt2")] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.LeftShoulder => TFGame.MenuAtlas["controls/" + text + "/lb"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Paddle1EXT=> TFGame.MenuAtlas["controls/" + text + "/p1"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Paddle2EXT=> TFGame.MenuAtlas["controls/" + text + "/p2"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Paddle3EXT => TFGame.MenuAtlas["controls/" + text + "/p3"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                Buttons.Paddle4EXT => TFGame.MenuAtlas["controls/" + text + "/p4"] ?? TFGame.MenuAtlas["controls/unknownButton"],
                _ => TFGame.MenuAtlas["controls/unknownButton"],
            };
        }
        catch
        {
            return TFGame.MenuAtlas["controls/unknownButton"];
        }
    }
}