using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall;

public sealed class DeleteMenu : Entity 
{
    private static readonly Color SelectionA = Calc.HexToColor("F87858");
    private static readonly Color SelectionB = Calc.HexToColor("F7BB59");
    private static readonly Color NotSelection = Calc.HexToColor("F8F8F8");

    private List<string> optionNames = new();
    private List<string> selectedOptionNames = new();
    private List<Action> optionActions = new();
    private Wiggler selectionWiggler;
    private MenuPanel panel;
    private Counter confirmCounter;
    private string title;
    private int optionIndex;
    private bool selectionFlash;
    private patch_MapScene map;
    private bool cantDelete;

    public DeleteMenu(patch_MapScene map, Vector2 position, int id) : base(position, 0)
    {
        this.map = map;
        confirmCounter = new Counter();
        confirmCounter.Set(10);

        selectionWiggler = Wiggler.Create(20, 4f);
        Add(selectionWiggler);
        title = "DELETE LEVEL";
        AddItem("YES", () => {
            var level = patch_GameData.AdventureWorldTowers[id];
            if (!patch_GameData.AdventureWorldTowersLoaded.Contains(level.StoredDirectory)) 
            {
                cantDelete = true;
                return;
            }
            patch_GameData.AdventureWorldTowers.Remove(level);
            patch_GameData.AdventureWorldTowersLoaded.Remove(level.StoredDirectory);
            patch_SaveData.AdventureActive = false;
            UploadMapButton.SaveLoaded();
            patch_GameData.ReloadCustomLevels();
            map.GotoAdventure();
            RemoveSelf();
        });
        AddItem("NO", () => {
            RemoveSelf();
            Visible = false;
            map.MapPaused = false;
        });

        panel = new MenuPanel(120, optionNames.Count * 10 + 30);
        Add(panel);
    }

    public override void Removed()
    {
        base.Removed();
        MenuInput.Clear();
        map.MapPaused = false;
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
                return;
            }
            
            if (MenuInput.Back)
            {
                RemoveSelf();
                Visible = false;
                map.MapPaused = false;
            }
        }
    }

    public override void Render()
    {
        base.Render();
        var pos = Position + new Vector2(0f, -panel.Height /2f - 8f);
        Draw.TextureCentered(TFGame.MenuAtlas["questResults/arrow"], pos + new Vector2(0f, -2f), Color.White);
        Draw.OutlineTextCentered(TFGame.Font, title, pos, Color.White, 2f);
        int num = (this.optionNames.Count - 1) * 14 + 20 + 20 - 1;
        Vector2 value = new Vector2(base.X, base.Y - (float)(num / 2) + 20f);
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
                color = (this.selectionFlash ? SelectionB : SelectionA);
            }
            else
            {
                color = NotSelection;
            }
            Draw.TextCentered(TFGame.Font, (this.optionIndex == i) ? this.selectedOptionNames[i] : this.optionNames[i], value + zero, color);
            value.Y += 14f;
        }
    }


    private void AddItem(string name, Action action)
    {
        this.optionNames.Add(name);
        this.selectedOptionNames.Add("> " + name);
        this.optionActions.Add(action);
    }
}