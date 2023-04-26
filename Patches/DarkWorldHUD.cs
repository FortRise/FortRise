// #pragma warning disable CS0626
// #pragma warning disable CS0108
// using System;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Monocle;

// namespace TowerFall;

// public class patch_DarkWorldHUD : DarkWorldHUD
// {
    // private float hudEase;
    // public patch_DarkWorldRoundLogic DarkWorld;
    // public patch_DarkWorldHUD(DarkWorldRoundLogic darkWorld) : base(darkWorld) {}

    // public extern void orig_Render();

    // public override void Render()
    // {
    //     orig_Render();
    //     var pointPos = (float)Math.Ceiling(MathHelper.Lerp(-29f, 6f, this.hudEase));
    //     Draw.OutlineTextJustify(TFGame.Font, DarkWorld.Points.ToString(), 
    //         new Vector2(pointPos, 25f), Color.White, 
    //         Color.Black, new Vector2(0f, 0.5f));
    // }
// }