using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall;

public class UIModal : Entity
{
    public Color SelectionA = Calc.HexToColor("F87858");
    public Color SelectionB = Calc.HexToColor("F7BB59");
    public Color NotSelection = Calc.HexToColor("F8F8F8");

    private int itemCount;
    private List<string> optionNames = new();
    private List<string> fillerNames = new();
    private List<string> selectedOptionNames = new();
    private List<Action> optionActions = new();
    private Wiggler selectionWiggler;
    private MenuPanel panel;
    private Counter confirmCounter;
    private int optionIndex;
    private Color fillerColor;
    private bool noTitle;

    public bool SelectionFlash;
    public bool AutoClose = true;
    public string Title;

    public Action OnBack;

    public UIModal(int layerIndex = -1) : this(new Vector2(160f, 120f), layerIndex) {}

    public UIModal(Vector2 position, int layerIndex = -1) : base(position, layerIndex) 
    {
        confirmCounter = new Counter();
        confirmCounter.Set(10);

        selectionWiggler = Wiggler.Create(20, 4f);
        Add(selectionWiggler);
    }


    public override void Added()
    {
        base.Added();
        Sounds.ui_pause.Play(160f);
        panel = new MenuPanel(120, itemCount * 10 + 30);
        Add(panel);
    }

    public override void Removed()
    {
        base.Removed();
        Sounds.ui_unpause.Play(160f);
        MenuInput.Clear();
    }

    public override void Update()
    {
        base.Update();
        MenuInput.Update();
        if (MenuInput.Down)
        {
            if (this.optionIndex < this.optionNames.Count - 1)
            {
                Sounds.ui_move1.Play(160f, 1f);
                optionIndex++;
                selectionWiggler.Start();
                return;
            }
        }
        else if (MenuInput.Up)
        {
            if (this.optionIndex > 0)
            {
                Sounds.ui_move1.Play(160f, 1f);
                optionIndex--;
                selectionWiggler.Start();
                return;
            }
        }
        else
        {
            if (confirmCounter)
            {
                confirmCounter.Update();
                return;
            }
            if (MenuInput.Confirm)
            {
                this.optionActions[this.optionIndex]();
                if (AutoClose)
                {
                    RemoveSelf();
                }
                return;
            }
            
            if (MenuInput.Back)
            {
                RemoveSelf();
                Visible = false;
                OnBack?.Invoke();
            }
        }
    }

    public override void Render()
    {
        Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
        base.Render();
        var pos = Position + new Vector2(0f, -panel.Height /2f - 8f);
        if (!noTitle)
            Draw.OutlineTextCentered(TFGame.Font, Title, pos, Color.White, 2f);
        int offset = !noTitle ? 20 : 0;

        int num = (itemCount - 1) * 14 + offset + offset - 1;
        var value = new Vector2(X, Y - (float)(num / 2) + offset);
        for (int i = 0; i < fillerNames.Count; i++) 
        {
            Draw.TextCentered(TFGame.Font, fillerNames[i], value, fillerColor);
            value.Y += 14f;
        }
        for (int i = 0; i < this.optionNames.Count; i++)
        {
            Vector2 zero = Vector2.Zero;
            if (i == this.optionIndex)
            {
                zero.X = this.selectionWiggler.Value * 3f;
            }
            Color color;
            if (this.optionIndex == i)
            {
                color = (this.SelectionFlash ? SelectionB : SelectionA);
            }
            else
            {
                color = NotSelection;
            }
            Draw.TextCentered(TFGame.Font, (this.optionIndex == i) ? this.selectedOptionNames[i] : this.optionNames[i], value + zero, color);
            value.Y += 14f;
        }
    }

    public UIModal SetTitle(string title) 
    {
        Title = title.ToUpperInvariant();
        return this;
    }

    public UIModal SetFillerColor(Color color) 
    {
        fillerColor = color;
        return this;
    }

    public UIModal SetSelectionColor(Color color) 
    {
        SelectionA = color;
        return this;
    }

    public UIModal SetAltSelectionColor(Color color) 
    {
        SelectionB = color;
        return this;
    }

    public UIModal SetColor(Color color) 
    {
        NotSelection = color;
        return this;
    }

    public UIModal HideTitle(bool hide) 
    {
        noTitle = hide;
        return this;
    }

    public UIModal SetOnBackCallBack(Action onBack) 
    {
        OnBack = onBack;
        return this;
    }

    public UIModal AddItem(string name, Action action)
    {
        name = name.ToUpperInvariant();
        optionNames.Add(name);
        selectedOptionNames.Add("> " + name);
        optionActions.Add(action);
        itemCount++;
        return this;
    }

    public UIModal AddFiller(string name) 
    {
        fillerNames.Add(name.ToUpperInvariant());
        itemCount++;
        return this;
    }
}
