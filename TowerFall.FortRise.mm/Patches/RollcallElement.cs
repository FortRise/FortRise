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

    public patch_RollcallElement(int playerIndex) : base(playerIndex)
    {
    }

    [MonoModReplace]
    private void ChangeSelectionLeft()
    {
        int characterIndex = CharacterIndex;
        int num = CharacterIndex;
        CharacterIndex = num - 1;

        while (TFGame.CharacterTaken(CharacterIndex) ||
            !SaveData.Instance.Unlocks.GetArcherUnlocked(CharacterIndex) ||
            IsArcherBlacklisted())
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
            IsArcherBlacklisted())
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

    private bool IsArcherBlacklisted()
    {
        var archer = ArcherRegistry.GetArcherEntry(CharacterIndex);

        if (archer is null)
        {
            var data = ArcherData.Archers[CharacterIndex];
            return FortRiseModule.Settings.BlacklistedArcher.Contains(data.Name0 + data.Name1);
        }

        return FortRiseModule.Settings.BlacklistedArcher.Contains(archer.Name);
    }
}