using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

public class UIVersusCustomMatch : UIModal 
{
    public int Count;
    public UIVersusCustomMatch(int startAt = 1) 
    {
        Count = startAt;
        HideTitle(true);
        AddFiller("SELECT GOALS");
        itemCount = 2;
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() 
    {
        base.Update();
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
    [MonoModIgnore]
    public void base_Render() 
    {
        base.Render();
    }

    public override void Update()
    {
        base_Update();
        MenuInput.Update();
        if (MenuInput.Left) 
        {
            if (Count <= 1)
                return;
            Count--;            
            Sounds.ui_click.Play(160f, 1);
            return;
        }
        if (MenuInput.Right) 
        {
            if (Count >= 25)
                return;
            Count++;
            Sounds.ui_click.Play(160f, 1);
            return;
        }
        if (MenuInput.Confirm || MenuInput.Back) 
        {
            RemoveSelf();
            Visible = false;
            OnBack?.Invoke();
        }
    }

    public override void Render()
    {
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        base_Render();
        int offset = 0;

        int num = (itemCount - 1) * 14 + offset + offset - 1;
        var value = new Vector2(X, Y - (float)(num / 2) + offset);
        for (int i = 0; i < FillerNames.Count; i++) 
        {
            Draw.TextCentered(TFGame.Font, FillerNames[i], value, Color.White);
            value.Y += 14f;
        }
        Draw.TextCentered(TFGame.Font, Count.ToString(), value, Color.White);
    }
}