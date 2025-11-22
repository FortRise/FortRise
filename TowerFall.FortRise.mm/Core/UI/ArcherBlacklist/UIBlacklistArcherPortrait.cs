using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

internal sealed class UIBlacklistArcherPortrait : MenuItem
{
    private ArcherData archerData;
    private Vector2 tweenFrom;
    private Vector2 tweenTo;

    public UIBlacklistArcherPortrait(Vector2 position, Vector2 tweenFrom, ArcherData archerData) : base(position)
    {
        this.archerData = archerData;
        this.tweenFrom = tweenFrom;
        tweenTo = position;
    }

    public override void Render()
    {
        Color color = Selected ? OptionsButton.SelectedColor : OptionsButton.NotSelectedColor;
        Color portraitColor;
        Subtexture currentPortrait;
        if (IsBlacklisted)
        {
            currentPortrait = archerData.Portraits.Lose;
            portraitColor = Color.Black * 0.5f;
        }
        else
        {
            currentPortrait = archerData.Portraits.Win;
            portraitColor = Color.White;
        }

        Draw.Texture(currentPortrait, Position, Color.White, 1f);
        Draw.Texture(currentPortrait, Position, portraitColor, 1f);
        Draw.HollowRect(
            new Rectangle((int)Position.X, (int)Position.Y,
            currentPortrait.Rect.Width, currentPortrait.Rect.Height),
            color);
    }

    public override void TweenIn()
    {
        Position = tweenFrom;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(tweenFrom, tweenTo, t.Eased);
        };
        Add(tween);
    }

    public override void TweenOut()
    {
        Vector2 start = Position;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 12, true);
        tween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(start, tweenFrom, t.Eased);
        };
        Add(tween);
    }

    protected override void OnConfirm()
    {
        Sounds.ui_click.Play();

        int idx = Array.IndexOf(ArcherData.Archers, archerData);
        var entry = ArcherRegistry.GetArcherEntry(idx);
        if (entry is null)
        {
            if (!FortRiseModule.Settings.BlacklistedArcher.Remove(new BlacklistArcher(archerData.Name0 + archerData.Name1, true)))
            {
                FortRiseModule.Settings.BlacklistedArcher.Add(new BlacklistArcher(archerData.Name0 + archerData.Name1, true));
            }
            return;
        }

        if (!FortRiseModule.Settings.BlacklistedArcher.Remove(new BlacklistArcher(entry.Name, false)))
        {
            FortRiseModule.Settings.BlacklistedArcher.Add(new BlacklistArcher(entry.Name, false));
        }
    }

    protected override void OnDeselect()
    {
    }

    protected override void OnSelect()
    {
        if (MainMenu is not null)
        {
            MainMenu.TweenUICameraToY(Math.Max(0f, Y - 120f), 10);
        }
    }

    private bool IsBlacklisted
    {
        get
        {
            int idx = Array.IndexOf(ArcherData.Archers, archerData);

            var entry = ArcherRegistry.GetArcherEntry(idx);
            if (entry is null)
            {
                return FortRiseModule.Settings.BlacklistedArcher.Contains(new BlacklistArcher(archerData.Name0 + archerData.Name1, true));
            }

            return FortRiseModule.Settings.BlacklistedArcher.Contains(new BlacklistArcher(entry.Name, false));
        }
    }
}