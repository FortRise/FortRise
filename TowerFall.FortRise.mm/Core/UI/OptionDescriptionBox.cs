using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using TowerFall;

namespace FortRise;

public sealed class OptionDescriptionBox : MenuItem
{
    private const float MaxTextWidth = 260f;
    private const float PaddingX = 8f;
    private const float PaddingY = 6f;
    private const float LineHeight = 10f;
    private const float BottomMargin = 6f;
    private const float CenterX = 160f;
    private const float ScreenHeight = 240f;

    private readonly List<OptionsButton> buttons;
    private string currentText;
    private string[] lines = [];
    private float panelWidth;

    public OptionDescriptionBox(List<OptionsButton> buttons) : base(Vector2.Zero)
    {
        this.buttons = buttons;
        Depth = -100;
    }

    public override void TweenIn() { }
    public override void TweenOut() { }
    protected override void OnSelect() { }
    protected override void OnDeselect() { }
    protected override void OnConfirm() { }

    public override void Render()
    {
        base.Render();

        var description = FindSelectedDescription();
        if (string.IsNullOrEmpty(description))
        {
            return;
        }

        if (description != currentText)
        {
            currentText = description;
            lines = TFGame.Font.WrapText(description, MaxTextWidth);
            panelWidth = 0f;
            foreach (var line in lines)
            {
                panelWidth = Math.Max(panelWidth, TFGame.Font.MeasureString(line).X);
            }
            panelWidth += PaddingX * 2f;
        }

        if (lines.Length == 0)
        {
            return;
        }

        float cameraY = MainMenu != null ? MainMenu.UILayer.Camera.Y : 0f;
        float panelHeight = lines.Length * LineHeight + PaddingY * 2f;
        float top = cameraY + ScreenHeight - BottomMargin - panelHeight;
        float left = CenterX - panelWidth / 2f;

        MenuPanel.DrawPanel(left, top - 3f, panelWidth + 5.5f, panelHeight);

        for (int i = 0; i < lines.Length; i++)
        {
            Draw.OutlineTextCentered(
                TFGame.Font,
                lines[i],
                new Vector2(CenterX, top + PaddingY + i * LineHeight + LineHeight / 2f),
                Color.White,
                Color.Black);
        }
    }

    private string FindSelectedDescription()
    {
        foreach (var button in buttons)
        {
            if (button.Selected)
            {
                return button.GetDescription();
            }
        }

        return null;
    }
}
