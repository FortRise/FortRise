using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_PlayerInput : PlayerInput
{
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
        int num = 0;
        if (!MainMenu.NoGamepads) 
        {
            MInput.UpdateDirectInput = false;
            MInput.UpdateXInput = true;
            for (int i = 0; i < 4; i++) 
            {
                if (MInput.XGamepads.Count <= i) 
                    break;
                TFGame.PlayerInputs[num] = new XGamepadInput(i);
                num++;
            }
        }

        if (num <= 3) 
        {
            if (SaveData.Instance.Keyboard == null || SaveData.Instance.Keyboard.Length == 0) 
            {
                TFGame.PlayerInputs[num] = new KeyboardInput();
                num++;
            }
            else 
            {
                for (int i = 0; i < SaveData.Instance.Keyboard.Length; i++)
                {
                    if (SaveData.Instance.Keyboard[i] == null)
                        continue;
                    
                    TFGame.PlayerInputs[num] = new KeyboardInput(SaveData.Instance.Keyboard[i], i);
                    num++;
                    if (num > 3)
                        break;
                }
            }
        }
        for (int i = num; i < 4; i++) 
        {
            TFGame.PlayerInputs[i] = null;
        }
        MenuInput.UpdateInputs();
        MenuButtons.Update();
    }
}