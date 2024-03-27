using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;


public class UILoader : Loader
{
    private Coroutine coroutine = new Coroutine();
    public bool Finished;
    public UILoader() : base(true)
    {
        Add(coroutine);
    }

    public void WaitWith(Action action) 
    {
        coroutine.Replace(WaitSequence(action));
    }

    private IEnumerator WaitSequence(Action action) 
    {
        while (!Finished)
            yield return 0;
        action?.Invoke();
    }

    public override void Update()
    {
        if (Finished)
            RemoveSelf();
        base.Update();
    }

    public override void Render()
    {
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.5f);
        base.Render();
        Draw.OutlineTextCentered(TFGame.Font, "LOADING", this.Position + new Vector2(0f, 22f), Color.White, Color.Black);
    }
}