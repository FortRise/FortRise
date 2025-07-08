using System;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_RollcallElement : RollcallElement
{
    private ArcherPortrait portrait;
    private Image rightArrow;
    private Image leftArrow;
    private ArcherData.ArcherTypes archerType;
    private int playerIndex;

    private Wiggler arrowWiggle;
    private Wiggler altWiggle;
    private StateMachine state;
    private float altEase;
    private Vector2 tweenTo;
    private Vector2 tweenFrom;
    private PlayerInput input;
    private SineWave arrowSine;
    private SineWave darkWorldAlphaSine;
    private bool isLocked;

    public patch_RollcallElement(int playerIndex) : base(playerIndex) { }

    [MonoModLinkTo("TowerFall.MenuItem", "System.Void .ctor(Microsoft.Xna.Framework.Vector2)")]
    public void base_ctor(Vector2 position) {}

    [MonoModReplace]
    [MonoModConstructor]
    public void ctor(int playerIndex)
    {
        base_ctor(GetPosition(playerIndex));

        bool shouldLock = playerIndex >= ArcherData.Archers.Length - FortRiseModule.Settings.BlacklistedArcher.Count;
        isLocked = shouldLock;

        this.playerIndex = playerIndex;
        tweenTo = Position;
        tweenFrom = GetTweenSource(playerIndex);

        if (!shouldLock)
        {
            input = TFGame.PlayerInputs[playerIndex];

            if (!TFGame.Players[playerIndex])
            {
                while (TFGame.CharacterTaken(CharacterIndex) ||
                    !SaveData.Instance.Unlocks.GetArcherUnlocked(CharacterIndex) ||
                    IsArcherBlacklisted(CharacterIndex))
                {
                    CharacterIndex += 1;
                }
            }
        }

        portrait = new ArcherPortrait(Vector2.Zero, TFGame.Characters[playerIndex], archerType, true);
        Add(portrait);
        portrait.Visible = !shouldLock;

        if (!TFGame.Players[playerIndex] && TFGame.AltSelect[playerIndex] == ArcherData.ArcherTypes.Secret)
        {
            TFGame.AltSelect[playerIndex] = ArcherData.ArcherTypes.Normal;
        }

        archerType = TFGame.AltSelect[playerIndex];

        rightArrow = new Image(TFGame.MenuAtlas["portraits/arrow"])
        {
            Visible = false
        };

        rightArrow.CenterOrigin();

        Add(rightArrow);
        leftArrow = new Image(TFGame.MenuAtlas["portraits/arrow"])
        {
            FlipX = true,
            Visible = false
        };

        leftArrow.CenterOrigin();

        Add(leftArrow);

        leftArrow.Y = rightArrow.Y = 60f;
        arrowWiggle = Wiggler.Create(20, 6f, Ease.CubeIn);

        Add(arrowWiggle);
        Add(arrowSine = new SineWave(120));

        altWiggle = Wiggler.Create(30, 4f);
        Add(altWiggle);

        state = new StateMachine(2);
        state.SetCallbacks(0, new Func<int>(NotJoinedUpdate), new Action(EnterNotJoined), new Action(LeaveNotJoined));
        state.SetCallbacks(1, new Func<int>(JoinedUpdate), new Action(EnterJoined), new Action(LeaveJoined));
        state.SetCallbacks(2, null);

        Add(state);

        HandleControlIcons();
        HandleState();

        if (TFGame.Players[playerIndex])
        {
            portrait.StartJoined();
        }

        if (archerType == ArcherData.ArcherTypes.Alt)
        {
            altEase = 1f;
        }

        GamePad.SetLightBarEXT((PlayerIndex)playerIndex, portrait.ArcherData.LightbarColor);
        rightArrow.Color = leftArrow.Color = Color.Lerp(ArcherData.Archers[playerIndex].ColorB, Color.White, 0.5f);

        Add(darkWorldAlphaSine = new SineWave(120));
    }

    [MonoModReplace]
    private void EnforceCharacterLock(int takenIndex)
    {
        if (state.State != 1 && TFGame.CharacterTaken(CharacterIndex) && !isLocked)
        {
            if (CharacterIndex == takenIndex)
            {
                ChangeSelectionRight();
            }
        }
    }

    [MonoModReplace]
    private void ChangeSelectionLeft()
    {
        int characterIndex = CharacterIndex;
        int num = CharacterIndex;
        CharacterIndex = num - 1;

        while (TFGame.CharacterTaken(CharacterIndex) ||
            !SaveData.Instance.Unlocks.GetArcherUnlocked(CharacterIndex) ||
            IsArcherBlacklisted(CharacterIndex))
        {
            num = CharacterIndex;
            CharacterIndex = num - 1;
        }

        if (characterIndex != CharacterIndex)
        {
            portrait.SetCharacter(CharacterIndex, archerType, -1);
            GamePad.SetLightBarEXT((PlayerIndex)playerIndex, portrait.ArcherData.LightbarColor);
            rightArrow.Color = leftArrow.Color = Color.Lerp(ArcherData.Archers[CharacterIndex].ColorB, Color.White, 0.5f);
        }
    }

    [MonoModReplace]
    private void ChangeSelectionRight()
    {
        int characterIndex = CharacterIndex;
        int num = CharacterIndex;
        CharacterIndex = num + 1;
        while (TFGame.CharacterTaken(CharacterIndex) ||
            !SaveData.Instance.Unlocks.GetArcherUnlocked(CharacterIndex) ||
            IsArcherBlacklisted(CharacterIndex))
        {
            num = CharacterIndex;
            CharacterIndex = num + 1;
        }
        if (characterIndex != CharacterIndex)
        {
            portrait.SetCharacter(CharacterIndex, archerType, 1);
            GamePad.SetLightBarEXT((PlayerIndex)playerIndex, portrait.ArcherData.LightbarColor);
            rightArrow.Color = leftArrow.Color = Color.Lerp(ArcherData.Archers[CharacterIndex].ColorB, Color.White, 0.5f);
        }
    }

    private bool IsArcherBlacklisted(int charIndex)
    {
        var archer = ArcherRegistry.GetArcherEntry(charIndex);

        if (archer is null)
        {
            var data = ArcherData.Archers[charIndex];
            return FortRiseModule.Settings.BlacklistedArcher.Contains(data.Name0 + data.Name1);
        }

        return FortRiseModule.Settings.BlacklistedArcher.Contains(archer.Name);
    }


    [MonoModIgnore]
    private extern void HandleControlIcons();

    [MonoModIgnore]
    private extern void HandleState();

    [MonoModIgnore]
    private extern int NotJoinedUpdate();

    [MonoModIgnore]
    private extern void EnterNotJoined();

    [MonoModIgnore]
    private extern void LeaveNotJoined();

    [MonoModIgnore]
    private extern int JoinedUpdate();

    [MonoModIgnore]
    private extern void EnterJoined();

    [MonoModIgnore]
    private extern void LeaveJoined();
}