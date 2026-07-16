using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall.Editor;

public class OverlayAttributeScene : patch_OverlayScene
{
    public OverlayAttributeScene(EditorScene editor, patch_Actor data) : base(editor, false)
    {
        InitActorAttributes(data);
    }

    private void InitActorAttributes(patch_Actor actor) 
    {
        ModeName = "ENTITY DATA";
        Remove(ModeUI);
        int yOffset = -60;
        foreach (var customData in actor.Attributes) 
        {
            yOffset += 60;
            if (actor.Data.AttributeSchemas is {}) 
            {
                if (actor.Data.AttributeSchemas.TryGetValue(customData.Key, out var v))
                {
                    var key = customData.Key.ToUpperInvariant();
                    var dv = customData.Value;
                    var cb = new OverlayComboBoxButton(
                        new Vector2(480, 270f + yOffset), key, dv, v, (s) => {
                        actor.Attributes[customData.Key] = s;
                    });
                    ModeUI.Add(cb);
                    continue;
                }
            }
            var titleKey = customData.Key.ToUpperInvariant();
            var defaultValue = customData.Value.ToUpperInvariant();
            var textBox = new OverlayTextBox(
                new Vector2(480, 270f + yOffset), titleKey, defaultValue, 100, s => {
                actor.Attributes[customData.Key] = s;
            });
            ModeUI.Add(textBox);
        }

        Add(ModeUI);
    }
}
public class OverlayComboBoxButton : EditorUI
{
    private Wiggler rotateWiggler;
    private Wiggler scaleWiggler;
    private string[] items;
    private string[] displayItems;
    private int selectedItem;
    private string key;

    public Action<string> onItemSelect;

    public OverlayComboBoxButton(
        Vector2 position, string key, string defaultValue, string[] items, Action<string> onItemSelect)
        : base(position, 300, 50, -150, -35)
    {
        this.onItemSelect = onItemSelect;
        this.key = key.ToUpperInvariant();
        this.items = items;
        var idx = items.IndexOf(defaultValue);
        if (idx != -1)
        {
            selectedItem = idx;
        }
        
        displayItems = new string[items.Length];

        for (int i = 0; i < items.Length; i += 1)
        {
            displayItems[i] = items[i].ToUpperInvariant();
        }

        rotateWiggler = Wiggler.Create(20, 4f, null, null, false, false);
        Add(rotateWiggler);

        scaleWiggler = Wiggler.Create(20, 4f, null, null, false, false);
        Add(scaleWiggler);
    }

    public override void Render()
    {
        Draw.HollowRect(Left, Top, Width, Height, Hovered ? (Color.Yellow * 0.5f) : (Color.DarkGray * 0.5f));

        Draw.TextCentered(TFGame.Font, key + ":", Position + new Vector2(0f, -22f), Color.White, 2f);
        Draw.TextCentered(
            TFGame.Font, 
            displayItems[selectedItem], 
            Position, 
            Color.White, 
            Vector2.One * (3f - 0.2f * scaleWiggler.Value), 
            0.06981317f * rotateWiggler.Value
        );
    }

    public override void OnMouseEnter()
    {
        base.OnMouseEnter();
        Sounds.ed_buttonMouse.Play(160f, 1f);
        rotateWiggler.Start();
    }

    public override void OnMouseClick(Vector2 localPosition)
    {
        base.OnMouseClick(localPosition);
        selectedItem = (selectedItem + 1) % items.Length;

        onItemSelect(items[selectedItem]);

        Sounds.ed_buttonClick.Play(160f, 1f);
        scaleWiggler.Start();
    }
}
