using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Text.Json.Serialization;

namespace TowerFall;

[Serializable]
public class GamepadConfig
{
    [JsonInclude]
    public string ButtonSet = "Automatic";

    [JsonInclude]
    public Buttons[] Left = [Buttons.DPadLeft];

    [JsonInclude]
    public Buttons[] Right = [Buttons.DPadRight];

    [JsonInclude]
    public Buttons[] Up = [Buttons.DPadUp];

    [JsonInclude]
    public Buttons[] Down = [Buttons.DPadDown];

    [JsonInclude]
    public Buttons[] Jump = [Buttons.A];

    [JsonInclude]
    public Buttons[] Shoot = [Buttons.X];

    [JsonInclude]
    public Buttons[] AltShoot = [Buttons.B];

    [JsonInclude]
    public Buttons[] Dodge = [Buttons.RightShoulder, Buttons.RightTrigger];

    [JsonInclude]
    public Buttons[] Arrows = [Buttons.Y];

    [JsonInclude]
    public Buttons[] MenuAlt = [Buttons.LeftShoulder, Buttons.LeftTrigger];

    [JsonInclude]
    public Buttons[] Start = [Buttons.Start];

    [JsonInclude]
    public float MoveXDeadzone = 0.5f;

    [JsonInclude]
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