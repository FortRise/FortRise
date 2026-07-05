using Monocle;
using MonoMod;

namespace TowerFall;


[MonoModPatch("TowerFall.PlayerInput")]
public abstract class PlayerInputAbstract
{
    public abstract Subtexture ArrowsIcon { get; }
    public abstract bool MenuArrows { get; }
    public abstract bool MenuArrowsCheck { get; }
}

public class patch_PlayerInput : PlayerInput
{
    [MonoModLinkTo("TowerFall.PlayerInput", "Monocle.Subtexture get_ArrowsIcon()")]
    [MonoModIgnore]
    public Subtexture get_ArrowsIcon_base() => null;

    [MonoModLinkTo("TowerFall.PlayerInput", "System.Boolean get_MenuArrows()")]
    [MonoModIgnore]
    public bool get_MenuArrows_base() => false;

    [MonoModLinkTo("TowerFall.PlayerInput", "System.Boolean get_MenuArrowsCheck()")]
    [MonoModIgnore]
    public bool get_MenuArrowsCheck_base() => false;

    [MonoModIgnore]    
    public override bool MenuConfirm => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuConfirmCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuBack => throw new System.NotImplementedException();
    [MonoModIgnore] 
    public override bool MenuStart => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuStartCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuAlt => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuAlt2 => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuUp => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuDown => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuRight => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuLeft => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuUpCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuDownCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuLeftCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuRightCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuAltCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuAlt2Check => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuBackCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuSkipReplay => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuSaveReplay => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool MenuSaveReplayCheck => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture Icon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture ConfirmIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture BackIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture AltIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture Alt2Icon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture StartIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture SkipReplayIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture SaveReplayIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture JumpIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture ShootIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture AltShootIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override Subtexture DodgeIcon => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override bool Attached => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override string Name => throw new System.NotImplementedException();
    [MonoModIgnore]
    public override string ID => throw new System.NotImplementedException();


    [MonoModIgnore]
    public extern override InputState GetState();

    [MonoModReplace]
    public static void AssignInputs()
    {
        TFGame.PlayerInputs = new PlayerInput[4];
        // resets everything
        for (int i = 0; i < 4; i++) 
        {
            TFGame.PlayerInputs[i] = null;
        }
        int connectedInputs = 0;
        if (!MainMenu.NoGamepads) 
        {
            MInput.UpdateDirectInput = false;
            MInput.UpdateXInput = true;
            foreach (var gamepad in patch_MInput.XGamepads)
            {
                TFGame.PlayerInputs[(int)gamepad.PlayerIndex] = new XGamepadInput(patch_MInput.XGamepads.IndexOf(gamepad));
                connectedInputs += 1;
            }
        }

        if (connectedInputs <= 3) 
        {
            if (SaveData.Instance.Keyboard == null || SaveData.Instance.Keyboard.Length == 0) 
            {
                for (int i = 0; i < TFGame.PlayerInputs.Length; i += 1)
                {
                    if (TFGame.PlayerInputs[i] is null)
                    {
                        TFGame.PlayerInputs[i] = new KeyboardInput();
                        break;
                    }
                }
            }
            else 
            {
                for (int i = 0; i < SaveData.Instance.Keyboard.Length; i++)
                {
                    if (SaveData.Instance.Keyboard[i] == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < TFGame.PlayerInputs.Length; j += 1)
                    {
                        if (TFGame.PlayerInputs[j] is null)
                        {
                            TFGame.PlayerInputs[j] = new KeyboardInput(SaveData.Instance.Keyboard[i], i);
                            connectedInputs++;
                            break;
                        }
                    }

                    if (connectedInputs > 3)
                        break;
                }
            }
        }
        // for (int i = num; i < 4; i++) 
        // {
        //     TFGame.PlayerInputs[i] = null;
        // }
        MenuInput.UpdateInputs();
        MenuButtons.Update();
    }
}