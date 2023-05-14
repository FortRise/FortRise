using System;
using Microsoft.Xna.Framework;

namespace TowerFall;

public class patch_BladeButton : BladeButton
{
    private Vector2 tweenTo, tweenFrom, selected;
    public patch_BladeButton(float y, string name, Action confirmAction) : base(y, name, confirmAction)
    {
    }

    public void SetX(float x) 
    {
        Position.X = x;
        tweenTo = Position;
        tweenFrom = Position + Vector2.UnitX * -(100f - x);
        selected = tweenTo + new Vector2(x + ((-x) + 30) , 0f);
    }
}