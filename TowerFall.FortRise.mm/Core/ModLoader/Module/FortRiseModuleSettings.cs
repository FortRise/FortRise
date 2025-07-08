using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

internal sealed class FortRiseModuleSettings : ModuleSettings
{
    public bool OldIntroLogo { get; set; }
    public List<string> BlacklistedArcher { get; set; } = new();

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Old Intro Logo", OldIntroLogo, (x) => OldIntroLogo = x);
        settings.Container.Add(new TextContainer.HeaderText("TOGGLE ARCHERS") { Scale = 1f });
        IndexProvider provider = new IndexProvider();
        for (int i = 0; i < ArcherData.Archers.Length; i += 4)
        {
            List<ArcherData> info = [];
            var archer = ArcherData.Archers[i];
            ArcherData archer2 = (i + 1) < ArcherData.Archers.Length ? ArcherData.Archers[i + 1] : null;
            ArcherData archer3 = (i + 2) < ArcherData.Archers.Length ? ArcherData.Archers[i + 2] : null;
            ArcherData archer4 = (i + 3) < ArcherData.Archers.Length ? ArcherData.Archers[i + 3] : null;
            info.Add(archer);

            if (archer2 is { } a2)
            {
                info.Add(a2);
            }

            if (archer3 is { } a3)
            {
                info.Add(a3);
            }

            if (archer4 is { } a4)
            {
                info.Add(a4);
            }

            settings.Container.Add(new BlacklistArcherPortraitColumn([.. info], provider));
        }
    }
}

// OOP be like
internal sealed class IndexProvider
{
    public int Index { get; set; }
}

internal sealed class BlacklistArcherPortraitColumn : TextContainer.Item
{
    private ArcherData[] archerDatas;
    private IndexProvider provider;
    public override int LineOffset => 60;

    public BlacklistArcherPortraitColumn(ArcherData[] archerData, IndexProvider provider)
    {
        this.archerDatas = archerData;
        this.provider = provider;
    }

    public override void OnSelected()
    {
        provider.Index = (int)MathHelper.Clamp(provider.Index, 0, archerDatas.Length - 1);
    }

    public override void ConfirmPressed()
    {
        Sounds.ui_click.Play();

        var archerData = archerDatas[provider.Index];
        int idx = Array.IndexOf(ArcherData.Archers, archerData);

        var entry = ArcherRegistry.GetArcherEntry(idx);
        if (entry is null)
        {
            if (!FortRiseModule.Settings.BlacklistedArcher.Remove(archerData.Name0 + archerData.Name1))
            {
                FortRiseModule.Settings.BlacklistedArcher.Add(archerData.Name0 + archerData.Name1);
            }
            return;
        }

        if (!FortRiseModule.Settings.BlacklistedArcher.Remove(entry.Name))
        {
            FortRiseModule.Settings.BlacklistedArcher.Add(entry.Name);
        }
    }

    public override void LeftPressed()
    {
        provider.Index = (int)MathHelper.Clamp(provider.Index - 1, 0, archerDatas.Length - 1);
    }

    public override void RightPressed()
    {
        provider.Index = (int)MathHelper.Clamp(provider.Index + 1, 0, archerDatas.Length - 1);
    }

    public override void Render(Vector2 position, bool selected)
    {
        Color color = OptionsButton.NotSelectedColor;

        int x = (int)position.X - 130;
        int y = (int)position.Y;

        for (int i = 0; i < archerDatas.Length; i++)
        {
            int posX = x + (i % 4 * 60);
            Vector2 pos = new Vector2(posX, y);
            var portrait = archerDatas[i];
            Color portraitBorderColor = color;
            if (Selected && provider.Index == i)
            {
                portraitBorderColor = OptionsButton.SelectedColor;
            }
            Subtexture icon = portrait.Portraits.Win;

            Color portraitBlockerColor = Color.Transparent;

            if (IsBlacklisted(i))
            {
                icon = portrait.Portraits.Lose;
                portraitBlockerColor = Color.Black * 0.5f;
            }


            Draw.Texture(icon, pos, Color.White, 1f);
            Draw.Texture(icon, pos, portraitBlockerColor, 1f);
            Draw.HollowRect(new Rectangle(posX, y, portrait.Portraits.Win.Rect.Width, portrait.Portraits.Win.Rect.Height), portraitBorderColor);
        }
    }

    private bool IsBlacklisted(int index)
    {
        var archerData = archerDatas[index];
        int idx = Array.IndexOf(ArcherData.Archers, archerData);

        var entry = ArcherRegistry.GetArcherEntry(idx);
        if (entry is null)
        {
            return FortRiseModule.Settings.BlacklistedArcher.Contains(archerData.Name0 + archerData.Name1);
        }

        return FortRiseModule.Settings.BlacklistedArcher.Contains(entry.Name);
    }
}

internal sealed class BlacklistArcherPortraitContainer : TextContainer.Item
{
    private List<ArcherData.PortraitInfo> archers = new List<ArcherData.PortraitInfo>();

    public BlacklistArcherPortraitContainer()
    {
        for (int i = 0; i < ArcherData.Archers.Length; i++)
        {
            var archer = ArcherData.Archers[i];
            archers.Add(archer.Portraits);
        }
    }

    public override void ConfirmPressed()
    {
        Sounds.ui_click.Play();
    }

    public override void Render(Vector2 position, bool selected)
    {
        int x = (int)position.X - 130;
        int y = (int)position.Y;
        Color color = Selected ? OptionsButton.SelectedColor : OptionsButton.NotSelectedColor;

        for (int i = 0; i < archers.Count; i++)
        {
            int posX = x + (i % 4 * 60);
            int posY = y + (i / 4 * 60);
            Vector2 pos = new Vector2(posX, posY);
            var portrait = archers[i];

            Draw.Texture(portrait.Win, pos, Color.White, 1f);
            Draw.HollowRect(new Rectangle(posX, posY, portrait.Win.Rect.Width, portrait.Win.Rect.Height), Color.White);
        }
    }
}