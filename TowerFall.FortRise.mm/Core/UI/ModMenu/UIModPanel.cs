using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

internal class UIModPanel : MenuItem
{
    private Vector2 tweenFrom;
    private Vector2 tweenTo;
    private ModItem modItem;

    public Action<ModItem> OnConfirmed;

    public UIModPanel(ModItem modItem, Vector2 position, Vector2 tweenFrom) : base(position)
    {
        this.modItem = modItem;

        this.tweenFrom = tweenFrom;
        tweenTo = Position;
    }

    [MonoModLinkTo("Monocle.Entity", "Update")]
    public void base_Update() {}

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
        OnConfirmed?.Invoke(modItem);
    }

    protected override void OnDeselect() {}

    protected override void OnSelect()
    {
        if (MainMenu is not null)
        {
            MainMenu.TweenUICameraToY(Math.Max(0f, Y - 120f), 10);
        }
    }

    public override void Render()
    {
        if (Selected)
        {
            Draw.HollowRect(Position.X - 4, Position.Y - 4, 200, 22, Color.Yellow);
        }

        Draw.TextureJustify(modItem.Icon, new Vector2(Position.X, Position.Y), Vector2.Zero);
        Draw.TextJustify(TFGame.Font, modItem.Name, Position + Vector2.UnitX * 20, Color.White, Vector2.Zero);
        Draw.TextJustify(TFGame.Font, modItem.Version, Position + new Vector2(20, 10), Color.DarkGray, Vector2.Zero);
    }

    public class ModItem 
    {
        private readonly Subtexture icon;
        private readonly string nameUpper;
        private readonly string versionUpper;

        public string Name => nameUpper;
        public string Version => versionUpper;
        public Subtexture Icon => icon;
        public ModuleMetadata Metadata { get; private set; }

        public ModItem(ModuleMetadata metadata) 
        {
            Metadata = metadata;
            if (!string.IsNullOrEmpty(metadata.DisplayName))
            {
                nameUpper = metadata.DisplayName.ToUpperInvariant();
            }
            else 
            {
                nameUpper = metadata.Name.ToUpperInvariant();
            }

            versionUpper = metadata.Version.ToString().ToUpperInvariant();

            if (RiseCore.ModuleManager.NameToIcon.TryGetValue(metadata.Name, out var icon))
            {
                this.icon = icon;
            }
            else 
            {
                this.icon = TFGame.MenuAtlas["variants/noArrows"];
            }
        }
    }
}
