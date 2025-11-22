using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public class UIArcherBlacklist(MainMenu main) : CustomMenuState(main)
{
    public override void Create()
    {
        const int GridAmount = 4;
        List<UIBlacklistArcherPortrait> portraits = [];
        int sum = 0;
        for (int i = 0; i < ArcherData.Archers.Length; i += 1)
        {
            var archer = ArcherData.Archers[i];
            int gridX = i % GridAmount;
            int gridY = i / GridAmount;

            int posX = gridX * 60;
            int posY = gridY * 60;

            var tweenFrom = (gridY & 1) == 0 ? new Vector2(posX + 400, posY + 50) : new Vector2(posX - 400, posY + 50);

            var portrait = new UIBlacklistArcherPortrait(new Vector2(posX + (160 - 110), posY + 50), tweenFrom, archer);
            portraits.Add(portrait);

            sum += posY;
        }

        for (int x = 0; x < portraits.Count; x += 1)
        {
            var portrait = portraits[x];

            if (x != 0)
            {
                portrait.LeftItem = portraits[x - 1];
            }

            if (x + 1 < portraits.Count)
            {
                portrait.RightItem = portraits[x + 1];
            }

            if (x - GridAmount >= 0)
            {
                portrait.UpItem = portraits[x - GridAmount];
            }

            if (x + GridAmount < portraits.Count)
            {
                portrait.DownItem = portraits[x + GridAmount];
            }
        }

        Main.MaxUICameraY = sum - 40;
        Main.Add(portraits);
        ((patch_MainMenu)Main).TweenBGCameraToY(2);
        ((patch_MainMenu)Main).ToStartSelected = portraits[0];
        Main.BackState = MainMenu.MenuState.Options;
    }

    public override void Destroy()
    {
    }
}
