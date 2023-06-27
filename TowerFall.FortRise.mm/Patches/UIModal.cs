using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall;

/// <summary>
/// A modal api that can be used to spawn a dialog that can be added in the scene.
/// </summary>
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
    private Color fillerColor = Color.White;
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

    /// <summary>
    /// Set the title of a modal.
    /// </summary>
    /// <param name="title">A title of the modal you want to set</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetTitle(string title) 
    {
        Title = title.ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Set the filler text a color.
    /// </summary>
    /// <param name="title">A color of a filler text you want to set</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetFillerColor(Color color) 
    {
        fillerColor = color;
        return this;
    }

    /// <summary>
    /// Set the selection text a color.
    /// </summary>
    /// <param name="color">A color of a selection text you want to set</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetSelectionColor(Color color) 
    {
        SelectionA = color;
        return this;
    }

    /// <summary>
    /// Set the alternate selection text a color.
    /// </summary>
    /// <param name="color">A color of a alternate selection text you want to set</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetAltSelectionColor(Color color) 
    {
        SelectionB = color;
        return this;
    }

    /// <summary>
    /// Set the default text color or unselected text color.
    /// </summary>
    /// <param name="color">A color of an unselected text you want to set</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetColor(Color color) 
    {
        NotSelection = color;
        return this;
    }

    /// <summary>
    /// Hides the title of a modal.
    /// </summary>
    /// <param name="hide">A value between true or false to hide the title</param>
    /// <returns>A UIModal context</returns>
    public UIModal HideTitle(bool hide) 
    {
        noTitle = hide;
        return this;
    }

    /// <summary>
    /// A callback when a user press the `BACK` button.
    /// </summary>
    /// <param name="onBack">A function or callback that will be called when the use press `BACK` button</param>
    /// <returns>A UIModal context</returns>
    public UIModal SetOnBackCallBack(Action onBack) 
    {
        OnBack = onBack;
        return this;
    }

    /// <summary>
    /// Add an item or button that a user can interact with.
    /// </summary>
    /// <param name="name">A name of the text in an item</param>
    /// <param name="action">A function or callback that will be called when the user press this item</param>
    /// <returns>A UIModal context</returns>
    public UIModal AddItem(string name, Action action)
    {
        name = name.ToUpperInvariant();
        optionNames.Add(name);
        selectedOptionNames.Add("> " + name);
        optionActions.Add(action);
        itemCount++;
        return this;
    }

    /// <summary>
    /// Add an extra item that represents just a normal plain text and cannot be interact. (This item will be in the priority first).
    /// </summary>
    /// <param name="name">A name of the text in an item</param>
    /// <returns>A UIModal context</returns>
    public UIModal AddFiller(string name) 
    {
        fillerNames.Add(name.ToUpperInvariant());
        itemCount++;
        return this;
    }
}
