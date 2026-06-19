using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FortRise;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.XGamepadInput")]
public class XGameInputExtends : PlayerInputAbstract
{
    public MInput.XGamepadData XGamepad;
    private Subtexture iconArrows;
    public override Subtexture ArrowsIcon => iconArrows;

    public override bool MenuArrows => XGamepad.Pressed(Buttons.Y);
    public override bool MenuArrowsCheck => XGamepad.Check(Buttons.Y);
}

[MonoModPatch("TowerFall.XGamepadInput")]
public class XGamepadInput : TowerFall.XGamepadInput
{
    public override bool MenuConfirm
    {
        [MonoModReplace]
        get => PressedButton(Config.Jump);
    }

    public override bool MenuConfirmCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Jump);
    }

    public override bool MenuBack
    {
        [MonoModReplace]
        get => PressedButton(Config.Shoot) || PressedButton(Config.AltShoot);
    }

    public override bool MenuBackCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Shoot) || CheckButtons(Config.AltShoot);
    }

    public override bool MenuStart
    {
        [MonoModReplace]
        get => PressedButton(Config.Start);
    }

    public override bool MenuStartCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Start);
    }

    public override bool MenuAlt
    {
        [MonoModReplace]
        get => PressedButton(Config.Dodge);
    }

    public override bool MenuAltCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Dodge);
    }

    public override bool MenuAlt2
    {
        [MonoModReplace]
        get => PressedButton(Config.MenuAlt);
    }

    public override bool MenuAlt2Check
    {
        [MonoModReplace]
        get => CheckButtons(Config.MenuAlt);
    }

    public override bool MenuSaveReplay
    {
        [MonoModReplace]
        get => MenuAlt2;
    }

    public override bool MenuSaveReplayCheck
    {
        [MonoModReplace]
        get => MenuAlt2Check;
    }

    public override bool MenuSkipReplay
    {
        [MonoModReplace]
        get => MenuStart;
    }

    public override bool MenuDown
    {
        [MonoModReplace]
        get => PressedButton(Config.Down) || XGamepad.LeftStickDownPressed(0.5f);
    }

    public override bool MenuDownCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Down) || XGamepad.LeftStickDownCheck(0.5f);
    }

    public override bool MenuUp
    {
        [MonoModReplace]
        get => PressedButton(Config.Up) || XGamepad.LeftStickUpPressed(0.5f);
    }

    public override bool MenuUpCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Up) || XGamepad.LeftStickUpCheck(0.5f);
    }

    public override bool MenuLeft
    {
        [MonoModReplace]
        get => PressedButton(Config.Left) || XGamepad.LeftStickLeftPressed(0.5f);
    }

    public override bool MenuLeftCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Left) || XGamepad.LeftStickLeftCheck(0.5f);
    }

    public override bool MenuRight
    {
        [MonoModReplace]
        get => PressedButton(Config.Right) || XGamepad.LeftStickRightPressed(0.5f);
    }

    public override bool MenuRightCheck
    {
        [MonoModReplace]
        get => CheckButtons(Config.Right) || XGamepad.LeftStickRightCheck(0.5f);
    }

    private Subtexture icon;
    private Subtexture iconMove;
    private Subtexture iconJump;
    private Subtexture iconShoot;
    private Subtexture iconAltShoot;
    private Subtexture iconArrows;
    private Subtexture iconDodge;
    private Subtexture iconStart;
    private Subtexture iconSkipReplay;
    private Subtexture iconConfirm;
    private Subtexture iconBack;
    private Subtexture iconAlt;
    private Subtexture iconAlt2;



    private string id;
    private string name;

	[MonoModPublic]
    public static Dictionary<string, XmlElement> ButtonIconMap;

	[MonoModPublic]
    public static Dictionary<string, XmlElement> ControllerInfoMap;
    public static string[] ButtonSets;

	public string AutoButtonSet
	{
		get
		{
	        if (string.IsNullOrEmpty(Config.ButtonSet) || Config.ButtonSet == "Automatic")
			{
				XmlElement controller = ControllerInfoMap.TryGetValue(id, out var val) ? val : ControllerInfoMap["null"];
				return controller.ChildText("buttonset", "default");
			}

			return Config.ButtonSet;
		}
	}

    public GamepadConfig Config;
    public XGamepadInput(int xGamepadID) : base(xGamepadID) {}

    [MonoModReplace]
    [MonoModConstructor]
    public void ctor(int xGamepadID) 
    {
        Config = ((patch_SaveData)SaveData.Instance).Gamepad[xGamepadID];

        XGamepadIndex = xGamepadID;
        XGamepad = MInput.XGamepads[xGamepadID];
        id = GamePad.GetGUIDEXT((PlayerIndex)xGamepadID);

        XmlElement controller = ControllerInfoMap.TryGetValue(id, out var val) ? val : ControllerInfoMap["null"];
        name = controller["name"].InnerText.ToUpperInvariant();

        ChangeButtonSet(Config.ButtonSet);
    }

    [MonoModReplace]
    public static void Init() 
    {
        ButtonIconMap = new Dictionary<string, XmlElement>();
        ControllerInfoMap = new Dictionary<string, XmlElement>();
        var iconConfig = Path.Combine(TFGame.OSSaveDir, "GamepadIcon_Config.xml");
        var nameConfig = Path.Combine(TFGame.OSSaveDir, "GamepadName_Config.xml");
        var extraiconConfig = Path.Combine(TFGame.OSSaveDir, "ExtraGamepadIcon_Config.xml");
        var extranameConfig = Path.Combine(TFGame.OSSaveDir, "ExtraGamepadName_Config.xml");

        var versionConfig = Path.Combine(TFGame.OSSaveDir, "version.txt");

        if (!File.Exists(iconConfig)) 
        {
            using var sw = new StreamWriter(iconConfig, false);
            sw.WriteLine(GamepadIconConfigContent);
        }
        if (!File.Exists(nameConfig)) 
        {
            using var sw = new StreamWriter(nameConfig, false);
            sw.WriteLine(GamepadNameConfigContent);
        }

        if (File.Exists(versionConfig))
        {
            using var txt = File.OpenText(versionConfig);
            if (SemanticVersion.TryParse(txt.ReadToEnd(), out var version))
            {
                if (version < RiseCore.FortRiseVersion)
                {
                    using (var sw = new StreamWriter(extraiconConfig, false))
                    {
                        sw.WriteLine(ExtrasGamepadIconConfigContent);
                    }

                    using (var sw = new StreamWriter(extranameConfig, false))
                    {
                        sw.WriteLine(ExtrasGamepadNameConfigContent);
                    }

                    File.WriteAllText(versionConfig, RiseCore.FortRiseVersion.ToString());
                }
            }
        }
        else
        {
            using (var sw = new StreamWriter(extraiconConfig, false))
            {
                sw.WriteLine(ExtrasGamepadIconConfigContent);
            }

            using (var sw = new StreamWriter(extranameConfig, false))
            {
                sw.WriteLine(ExtrasGamepadNameConfigContent);
            }

            File.WriteAllText(versionConfig, RiseCore.FortRiseVersion.ToString());
        }

        if (!File.Exists(extraiconConfig)) 
        {
            using var sw = new StreamWriter(extraiconConfig, false);
            sw.WriteLine(ExtrasGamepadIconConfigContent);
        }
        if (!File.Exists(extranameConfig)) 
        {
            using var sw = new StreamWriter(extranameConfig, false);
            sw.WriteLine(ExtrasGamepadNameConfigContent);
        }
        
        var vanillaButtonLists = Calc.LoadXML(iconConfig)["buttonlists"].GetElementsByTagName("buttonlist");
        var vanillaControllers = Calc.LoadXML(nameConfig)["controllers"].GetElementsByTagName("controller");

        var extraButtonLists = Calc.LoadXML(extraiconConfig)["buttonlists"].GetElementsByTagName("buttonlist");
        var extraContollers = Calc.LoadXML(extranameConfig)["controllers"].GetElementsByTagName("controller");

        InitIcons(vanillaButtonLists);
        InitControllers(vanillaControllers);

        InitIcons(extraButtonLists);
        InitControllers(extraContollers);

        ButtonSets = [.. ButtonIconMap.Keys];


        static void InitControllers(XmlNodeList controllers)
        {
            foreach (XmlElement controller in controllers) 
            {
                try 
                {
                    var guid = controller.GetElementsByTagName("guid");
                    foreach (XmlElement id in guid) 
                    {
                        ControllerInfoMap.Add(id.InnerText, controller);
                    }
                }
                catch (Exception e)
                {
                    RiseCore.logger.LogError("[XGamepadInput] Controllers parsed error: {e}", e.ToString());
                }
            }
        }

        static void InitIcons(XmlNodeList buttons)
        {
            foreach (XmlElement button in buttons) 
            {
                try 
                {
                    ButtonIconMap.Add(button.GetElementsByTagName("name")[0].InnerText, button);
                }
                catch (Exception e)
                {
                    RiseCore.logger.LogError("[XGamepadInput] Button List parsed error: {e}", e.ToString());
                }
            }
        }
    }

    public void ChangeButtonSet(string buttonSet)
    {
        Config.ButtonSet = buttonSet;

        if (string.IsNullOrEmpty(buttonSet) || buttonSet == "Automatic")
        {
            XmlElement controller = ControllerInfoMap.TryGetValue(id, out var val) ? val : ControllerInfoMap["null"];
            buttonSet = controller.ChildText("buttonset", "default");
        }

        XmlElement xmlElement = ButtonIconMap.TryGetValue(buttonSet, out XmlElement value) ? value : null;
        icon = ReadPlayerIcon(xmlElement, XGamepadIndex);
        iconMove = ReadButtonIcon(xmlElement, "move");

        RefreshButton();
    }

    public void RefreshButton()
    {
        string buttonSet = Config.ButtonSet;
        if (string.IsNullOrEmpty(buttonSet) || buttonSet == "Automatic")
        {
            XmlElement controller = ControllerInfoMap.TryGetValue(id, out var val) ? val : ControllerInfoMap["null"];
            buttonSet = controller.ChildText("buttonset", "default");
        }

        iconJump = GamepadConfig.GetIcon(buttonSet, Config.Jump[0]);
        iconShoot = GamepadConfig.GetIcon(buttonSet, Config.Shoot[0]);
        iconAltShoot = GamepadConfig.GetIcon(buttonSet, Config.AltShoot[0]);
        iconArrows = GamepadConfig.GetIcon(buttonSet, Config.Arrows[0]);
        iconDodge = GamepadConfig.GetIcon(buttonSet, Config.Dodge[0]);
        iconStart = GamepadConfig.GetIcon(buttonSet, Config.Start[0]);
        iconConfirm = GamepadConfig.GetIcon(buttonSet, Config.Jump[0]);
        iconBack = GamepadConfig.GetIcon(buttonSet, Config.AltShoot[0]);
        iconAlt = GamepadConfig.GetIcon(buttonSet, Config.Dodge[0]);
        iconAlt2 = GamepadConfig.GetIcon(buttonSet, Config.MenuAlt[0]);
        iconSkipReplay = GamepadConfig.GetIcon(buttonSet, Config.Start[0]);
    }

    [MonoModReplace]
    public override InputState GetState()
    {
        MInput.XGamepadData xgamepad = XGamepad;
        Vector2 vector = xgamepad.DPad;
        if (vector == Vector2.Zero)
        {
            vector = xgamepad.GetLeftStick();
        }
        return new InputState
        {
            MoveX = (Math.Abs(vector.X) < Config.MoveXDeadzone) ? 0 : Math.Sign(vector.X),
            MoveY = (Math.Abs(vector.Y) < Config.MoveYDeadzone) ? 0 : Math.Sign(vector.Y),
            AimAxis = (vector.LengthSquared() < 0.09f) ? Vector2.Zero : vector,
            JumpCheck = CheckButtons(Config.Jump),
            JumpPressed = PressedButton(Config.Jump),
            ShootCheck = CheckButtons(Config.Shoot),
            ShootPressed = PressedButton(Config.Shoot),
            AltShootCheck = CheckButtons(Config.AltShoot),
            AltShootPressed = PressedButton(Config.AltShoot),
            DodgeCheck = CheckButtons(Config.Dodge) || CheckButtons(Config.MenuAlt),
            DodgePressed = PressedButton(Config.Dodge) || PressedButton(Config.MenuAlt),
            ArrowsPressed = PressedButton(Config.Arrows)
        };
    }

    private bool CheckButtons(Buttons[] buttons)
    {
        for (int i = 0; i < buttons.Length; i += 1)
        {
            var button = buttons[i];
            switch (button)
            {
                case Buttons.LeftTrigger:
                    if (XGamepad.LeftTriggerCheck(0.1f))
                    {
                        return true;
                    }
                    break;
                case Buttons.RightTrigger:
                    if (XGamepad.RightTriggerCheck(0.1f))
                    {
                        return true;
                    }
                    break;
                default:
                    if (XGamepad.Check(buttons[i]))
                    {
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    private bool PressedButton(Buttons[] buttons)
    {
        for (int i = 0; i < buttons.Length; i += 1)
        {
            var button = buttons[i];
            switch (button)
            {
                case Buttons.LeftTrigger:
                    if (XGamepad.LeftTriggerPressed(0.1f))
                    {
                        return true;
                    }
                    break;
                case Buttons.RightTrigger:
                    if (XGamepad.RightTriggerPressed(0.1f))
                    {
                        return true;
                    }
                    break;
                default:
                    if (XGamepad.Pressed(buttons[i]))
                    {
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    [MonoModReplace]
    private static Subtexture ReadPlayerIcon(XmlElement config, int id) 
    {
        var text = (id + 1).ToString();
        if (config == null || config["player"] == null)
        {
            return TFGame.MenuAtlas["controls/generic/player" + text];
        }
    
        var text2 = "controls/" + config["player"].InnerText + text;
        if (TFGame.MenuAtlas.Contains(text2))
        {
            return TFGame.MenuAtlas[text2];
        }

        var text3 = "controls/generic/player" + text;
        if (!TFGame.MenuAtlas.Contains(text3))
        {
            // prevents potential crashes
            return TFGame.MenuAtlas["controls/generic/player1"];
        }

        return TFGame.MenuAtlas["controls/generic/player" + text];
    }

    [MonoModReplace]
    private static Subtexture ReadButtonIcon(XmlElement config, string name) 
    {
        if (config == null || config[name] == null)
        {
            return TFGame.MenuAtlas["controls/unknownButton"];
        }
        
        var text = "controls/" + config[name].InnerText;
        if (!TFGame.MenuAtlas.Contains(text))
        {
            return TFGame.MenuAtlas["controls/unknownButton"];
        }

        return TFGame.MenuAtlas[text];
    }

    private const string GamepadNameConfigContent = """
<!-- GamepadName_Config.xml -->
<controllers>
	<!-- Unknown devices -->
	<controller>
		<name>Generic</name>
		<guid>null</guid>
		<buttonset>XB360</buttonset>
	</controller>

	<!-- XInput devices -->
	<controller>
		<name>XInput</name>
		<guid>xinput</guid>
		<buttonset>XB360</buttonset>
	</controller>

	<!-- Xbox 360 -->
	<controller>
		<name>Xbox 360</name>
		<guid>5e048e02</guid>
		<guid>5e04a102</guid>
		<guid>380726b7</guid>
		<guid>5e041907</guid>
		<guid>5e049102</guid>
		<buttonset>XB360</buttonset>
	</controller>

	<!-- F310 -->
	<controller>
		<name>F310</name>
		<guid>6d041dc2</guid>
		<buttonset>XB360</buttonset>
	</controller>

	<!-- DualShock 4 -->
	<controller>
		<name>DualShock 4</name>
		<guid>4c05c405</guid>
		<buttonset>DS4</buttonset>
	</controller>

	<!-- DualShock 3 -->
	<controller>
		<name>DualShock 3</name>
		<guid>c0110055</guid>
		<buttonset>DS3</buttonset>
	</controller>

	<!-- DualShock 2 -->
	<controller>
		<name>DualShock 2</name>
		<guid>10080300</guid>
		<buttonset>DS2</buttonset>
	</controller>

	<!-- SNES -->
	<controller>
		<name>SNES</name>
		<guid>8f0e1303</guid>
		<buttonset>SNES</buttonset>
	</controller>

	<!-- NES -->
	<controller>
		<name>NES</name>
		<guid>bd12150d</guid>
		<buttonset>NES</buttonset>
	</controller>

	<!-- A.N.N.E -->
	<controller>
		<name>A.N.N.E.</name>
		<guid>79001100</guid>
		<buttonset>SNES</buttonset>
	</controller>

	<!-- iBuffalo -->
	<controller>
		<name>iBuffalo</name>
		<guid>83056020</guid>
		<buttonset>SNES</buttonset>
	</controller>

	<!-- Fighting Stick 3 -->
	<controller>
		<name>Fighting Stick 3</name>
		<guid>0d0f1000</guid>
		<buttonset>FS3</buttonset>
	</controller>

	<!-- Logitech Rumblepad 2 -->
	<controller>
		<name>Logitech Cordless Rumblepad 2</name>
		<guid>6d0419c2</guid>
		<buttonset>Rumblepad2</buttonset>
	</controller>

	<!-- Logitech Dual Action -->
	<controller>
		<name>Dual Action</name>
		<guid>6d0416c2</guid>
		<buttonset>DualAction</buttonset>
	</controller>
	<!-- Users can add controllers here. -->
</controllers>
""";

    private const string GamepadIconConfigContent = """
<!-- GamepadIcon_Config.xml -->
<buttonlists>
	<!-- Xbox 360 -->
	<buttonlist>
		<name>XB360</name>
		<player>xb360/player</player>
		<move>xb360/stick</move>
		<jump>xb360/a</jump>
		<shoot>xb360/x</shoot>
		<altShoot>xb360/b</altShoot>
		<arrows>xb360/y</arrows>
		<dodge>xb360/rt</dodge>
		<start>xb360/start</start>
		<confirm>xb360/a</confirm>
		<back>xb360/b</back>
		<alt>xb360/rt</alt>
		<alt2>xb360/lt</alt2>
		<skip_replay>xb360/start</skip_replay>
	</buttonlist>

	<!-- DualShock 5 -->
	<buttonlist>
		<name>DS5</name>
		<player>dualshock/player</player>
		<move>dualshock/dpad</move>
		<jump>dualshock/x</jump>
		<shoot>dualshock/square</shoot>
		<altShoot>dualshock/circle</altShoot>
		<arrows>dualshock/triangle</arrows>
		<dodge>dualshock/r2</dodge>
		<start>dualshock/x</start>
		<confirm>dualshock/x</confirm>
		<back>dualshock/circle</back>
		<alt>dualshock/r2</alt>
		<alt2>dualshock/l2</alt2>
		<skip_replay>dualshock/psButton</skip_replay>
	</buttonlist>

	<!-- DualShock 4 -->
	<buttonlist>
		<name>DS4</name>
		<player>dualshock/player</player>
		<move>dualshock/dpad</move>
		<jump>dualshock/x</jump>
		<shoot>dualshock/square</shoot>
		<altShoot>dualshock/circle</altShoot>
		<arrows>dualshock/triangle</arrows>
		<dodge>dualshock/r2</dodge>
		<start>dualshock/x</start>
		<confirm>dualshock/x</confirm>
		<back>dualshock/circle</back>
		<alt>dualshock/r2</alt>
		<alt2>dualshock/l2</alt2>
		<skip_replay>dualshock/psButton</skip_replay>
	</buttonlist>

	<!-- DualShock 3 -->
	<buttonlist>
		<name>DS3</name>
		<player>dualshock/player</player>
		<move>dualshock/dpad</move>
		<jump>dualshock/x</jump>
		<shoot>dualshock/square</shoot>
		<altShoot>dualshock/circle</altShoot>
		<arrows>dualshock/triangle</arrows>
		<dodge>dualshock/r2</dodge>
		<start>dualshock/psButton</start>
		<confirm>dualshock/x</confirm>
		<back>dualshock/circle</back>
		<alt>dualshock/r2</alt>
		<alt2>dualshock/l2</alt2>
		<skip_replay>dualshock/psButton</skip_replay>
	</buttonlist>

	<!-- DualShock 2 -->
	<buttonlist>
		<name>DS2</name>
		<player>dualshock/player</player>
		<move>dualshock/dpad</move>
		<jump>dualshock/x</jump>
		<shoot>dualshock/square</shoot>
		<altShoot>dualshock/circle</altShoot>
		<arrows>dualshock/triangle</arrows>
		<dodge>dualshock/r2</dodge>
		<start>dualshock/start</start>
		<confirm>dualshock/x</confirm>
		<back>dualshock/circle</back>
		<alt>dualshock/r2</alt>
		<alt2>dualshock/l2</alt2>
		<skip_replay>dualshock/start</skip_replay>
	</buttonlist>

	<!-- SNES -->
	<buttonlist>
		<name>SNES</name>
		<player>snes/player</player>
		<move>snes/dpad</move>
		<jump>snes/b</jump>
		<shoot>snes/y</shoot>
		<altShoot>snes/a</altShoot>
		<arrows>snes/x</arrows>
		<dodge>snes/r</dodge>
		<start>snes/start</start>
		<confirm>snes/b</confirm>
		<back>snes/a</back>
		<alt>snes/r</alt>
		<alt2>snes/l</alt2>
		<skip_replay>snes/start</skip_replay>
	</buttonlist>

	<!-- NES -->
	<buttonlist>
		<name>NES</name>
		<player>snes/player</player>
		<move>snes/dpad</move>
		<jump>snes/a</jump>
		<shoot>snes/b</shoot>
		<arrows>snes/x</arrows>
		<dodge>snes/select</dodge>
		<start>snes/start</start>
		<confirm>snes/a</confirm>
		<back>snes/b</back>
		<alt>snes/select</alt>
		<alt2>noButton</alt2>
		<skip_replay>snes/start</skip_replay>
	</buttonlist>

	<!-- Fighting Stick 3 -->
	<buttonlist>
		<name>FightingStick3</name>
		<player>dualshock/player</player>
		<move>dualshock/stick</move>
		<jump>dualshock/circle</jump>
		<shoot>dualshock/x</shoot>
		<arrows>dualshock/triangle</arrows>
		<dodge>dualshock/l2</dodge>
		<start>dualshock/start</start>
		<confirm>dualshock/circle</confirm>
		<back>dualshock/x</back>
		<alt>xb360/rt</alt>
		<alt2>xb360/lt</alt2>
		<skip_replay>dualshock/start</skip_replay>
	</buttonlist>

	<!-- Logitech Rumblepad 2 -->
	<buttonlist>
		<name>Rumblepad2</name>
		<player>generic/player</player>
		<move>generic/stick</move>
		<jump>generic/2</jump>
		<shoot>generic/1</shoot>
		<altShoot>generic/3</altShoot>
		<arrows>generic/4</arrows>
		<dodge>generic/shoulder6</dodge>
		<start>generic/start</start>
		<confirm>generic/2</confirm>
		<back>generic/3</back>
		<alt>generic/shoulder5</alt>
		<alt2>generic/shoulder6</alt2>
		<skip_replay>generic/start</skip_replay>
	</buttonlist>

	<!-- Logitech Dual Action -->
	<buttonlist>
		<name>DualAction</name>
		<player>generic/player</player>
		<move>generic/dpad</move>
		<jump>generic/2</jump>
		<shoot>generic/1</shoot>
		<altShoot>generic/3</altShoot>
		<arrows>generic/4</arrows>
		<dodge>generic/shoulder6</dodge>
		<start>generic/start</start>
		<confirm>generic/2</confirm>
		<back>generic/3</back>
		<alt>generic/shoulder6</alt>
		<alt2>generic/shoulder5</alt2>
		<skip_replay>generic/start</skip_replay>
	</buttonlist>
	<!-- Users can add buttonlists here. -->

</buttonlists>
""";

    private const string ExtrasGamepadNameConfigContent = """
<controllers> 
	<!-- DO NOT ADD ANYTHING HERE. -->
	<!-- THIS WILL BE OVERWRITTEN BY FORTRISE PER EVERY UPDATE. -->
	<!-- USE THE ORIGINAL GamepadIcon_Config.xml INSTEAD. -->

	<!-- 8BitDo Controller -->
    <controller>
        <name>8BitDo Ultimate</name>
        <guid>c82d1130</guid>
        <guid>c82d1230</guid>
        <guid>c82d1330</guid>
		<buttonset>XB360</buttonset>
    </controller>

    <controller>
        <name>8BitDo Ultimate C</name>
        <guid>c82d1530</guid>
        <guid>c82d1630</guid>
        <guid>c82d1730</guid>
		<buttonset>XB360</buttonset>
    </controller>

    <controller>
        <name>8BitDo Ultimate 2c</name>
        <guid>c82d1b30</guid>
        <guid>c82d1c30</guid>
        <guid>c82d1d30</guid>
		<buttonset>XB360</buttonset>
    </controller>

    <controller>
        <name>8BitDo Ultimate 2</name>
        <guid>c82d1260</guid>
		<buttonset>XB360</buttonset>
    </controller>

	<!-- DualSense -->
    <controller>
        <name>DualSense</name>
        <guid>4c05e60c</guid>
		<buttonset>DS5</buttonset>
    </controller>

	<!-- Steam Deck -->
    <controller>
        <name>Steam Deck</name>
        <guid>de280512</guid>
        <guid>de280511</guid>
        <guid>de280611</guid>
		<buttonset>Steam</buttonset>
    </controller>

	<!-- Nintendo Switch -->
	<controller>
		<name>Joy-Con</name>
		<guid>7e050620</guid>
		<guid>7e050720</guid>
		<buttonset>Switch</buttonset>
	</controller>

	<controller>
		<name>Nintendo Switch</name>
		<guid>ec11e1a7</guid>
		<buttonset>SwitchDual</buttonset>
	</controller>

	<controller>
		<name>Nintendo Switch Pro</name>
		<guid>7e056920</guid>
		<guid>7e050920</guid>
		<buttonset>SwitchPro</buttonset>
	</controller>

	<!-- Ouya -->
	<controller>
		<name>Ouya</name>
		<guid>36280100</guid>
		<buttonset>Ouya</buttonset>
	</controller>
</controllers>   
""";

    private const string ExtrasGamepadIconConfigContent = """
<buttonlists> 
	<!-- DO NOT ADD ANYTHING HERE. -->
	<!-- THIS WILL BE OVERWRITTEN BY FORTRISE PER EVERY UPDATE. -->
	<!-- USE THE ORIGINAL GamepadIcon_Config.xml INSTEAD. -->

	<!-- Ouya -->
	<buttonlist>
		<name>Ouya</name>
		<player>ouya/icon</player>
		<move>ouya/move</move>
		<jump>ouya/confirm</jump>
		<shoot>ouya/shoot</shoot>
		<altShoot>ouya/back</altShoot>
		<arrows>ouya/replaySkip</arrows>
		<dodge>ouya/alt</dodge>
		<start>ouya/start</start>
		<confirm>ouya/confirm</confirm>
		<back>ouya/back</back>
		<alt>ouya/alt</alt>
		<alt2>ouya/alt2</alt2>
		<skip_replay>ouya/replaySkip</skip_replay>
	</buttonlist>

	<!-- Steam Deck -->
	<buttonlist>
		<name>Steam</name>
		<player>steam/player</player>
		<move>steam/dpad</move>
		<jump>steam/a</jump>
		<shoot>steam/x</shoot>
		<altShoot>steam/b</altShoot>
		<arrows>steam/y</arrows>
		<dodge>steam/rt</dodge>
		<start>steam/start</start>
		<confirm>steam/a</confirm>
		<back>steam/b</back>
		<alt>steam/rt</alt>
		<alt2>steam/lt</alt2>
		<skip_replay>steam/start</skip_replay>
	</buttonlist>

	<!-- Switch -->
	<buttonlist>
		<name>Switch</name>
		<player>switch/player</player>
		<move>switch/stick</move>
		<jump>switch/b</jump>
		<shoot>switch/y</shoot>
		<altShoot>switch/a</altShoot>
		<arrows>switch/x</arrows>
		<dodge>switch/rt</dodge>
		<start>switch/start</start>
		<confirm>switch/b</confirm>
		<back>switch/a</back>
		<alt>switch/rt</alt>
		<alt2>switch/lt</alt2>
		<skip_replay>switch/start</skip_replay>
	</buttonlist>

	<!-- Switch Dual -->
	<buttonlist>
		<name>SwitchDual</name>
		<player>switchdual/player</player>
		<move>switch/stick</move>
		<jump>switch/b</jump>
		<shoot>switch/y</shoot>
		<altShoot>switch/a</altShoot>
		<arrows>switch/x</arrows>
		<dodge>switch/rt</dodge>
		<start>switch/start</start>
		<confirm>switch/b</confirm>
		<back>switch/a</back>
		<alt>switch/rt</alt>
		<alt2>switch/lt</alt2>
		<skip_replay>switch/start</skip_replay>
	</buttonlist>

	<!-- Switch Pro -->
	<buttonlist>
		<name>SwitchPro</name>
		<player>switchpro/player</player>
		<move>switch/stick</move>
		<jump>switch/b</jump>
		<shoot>switch/y</shoot>
		<altShoot>switch/a</altShoot>
		<arrows>switch/x</arrows>
		<dodge>switch/rt</dodge>
		<start>switch/start</start>
		<confirm>switch/b</confirm>
		<back>switch/a</back>
		<alt>switch/rt</alt>
		<alt2>switch/lt</alt2>
		<skip_replay>switch/start</skip_replay>
	</buttonlist>
</buttonlists>   
""";
}