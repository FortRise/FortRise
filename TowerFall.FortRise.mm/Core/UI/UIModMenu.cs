using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

public class UIModMenu : CustomMenuState
{
    private List<string> tagDisabled;
    public UIModMenu(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        tagDisabled ??= [];
        // if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
        // {
        //     var modButton = new TextContainer.ButtonText("UPDATE FORTRISE");
        //     modButton.Pressed(RiseCore.UpdateChecks.OpenFortRiseUpdateURL);
        //     textContainer.Add(modButton);
        // }

        ModContainer container = new ModContainer(new Vector2(81, 60));

        ButtonBox toggleMods = new ButtonBox(new Rectangle(81, 39, 128, 18), "TOGGLE MODS", new Vector2(81, 260));
        toggleMods.OnConfirmed = () => Main.State = ModRegisters.MenuState<UIModToggler>();
        toggleMods.DownItem = container;
        Main.Add(toggleMods);

        container.UpItem = toggleMods;
        container.OnConfirmed = (meta) => 
        {
            var fortModule = RiseCore.ModuleManager.InternalFortModules
                .Where(x => x.Meta is not null)
                .Where(x => x.Meta.ToString() == meta.ToString())
                .FirstOrDefault();
            if (fortModule == null)
            {
                Sounds.ui_invalid.Play(160f, 1f);
                return;
            }

            (Main as patch_MainMenu).CurrentModule = fortModule;
            Main.CanAct = true;
            Main.State = ModRegisters.MenuState<UIModOptions>();
        };

        Refresh(container);


        ButtonBox tags = new ButtonBox(new Rectangle(0, 0, 52, 240), "", new Vector2(-100, 0));
        Main.Add(tags);

        int y = 0;
        var list = new List<ButtonBox>();
        foreach (var tag in RiseCore.ModuleManager.GetAllTags())
        {
            ButtonBox tagButton = new ButtonBox(new Rectangle(2, y + 39, 46, 15), tag.ToUpperInvariant(), new Vector2(-100, y + 39));
            tagButton.RightItem = container;
            tagButton.OnConfirmed = () => 
            {
                if (!tagDisabled.Remove(tag))
                {
                    tagDisabled.Add(tag);
                }
                Refresh(container);
            };
            list.Add(tagButton);
            y += 17;
        }

        if (list.Count > 0)
        {
            container.LeftItem = list[0];
            toggleMods.LeftItem = list[0];
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];
                if (i == 0)
                {
                    if (i != list.Count - 1)
                    {
                        b.DownItem = list[i + 1];
                    }
                }
                else if (i == list.Count - 1)
                {
                    b.UpItem = list[i - 1];
                }
                else 
                {
                    b.UpItem = list[i - 1];
                    b.DownItem = list[i + 1];
                }
            }
            Main.Add(list);
        }

        Main.Add(container);
        (Main as patch_MainMenu).ToStartSelected = toggleMods;

        Main.BackState = MainMenu.MenuState.Main;
        Main.TweenUICameraToY(1);
    }

    public override void Destroy()
    {
        if ((Main as patch_MainMenu).switchTo != ModRegisters.MenuState<UIModOptions>())
        {
            Main.SaveOnTransition = true;
            foreach (var mod in RiseCore.ModuleManager.InternalFortModules) 
            {
                if (mod.InternalSettings == null)
                    continue;
                mod.SaveSettings();
            }
        }
    }

    private IReadOnlyList<ModuleMetadata> GetMods() 
    {
        if (tagDisabled.Count > 0)
        {
            return [.. RiseCore.ModuleManager.InternalModuleMetadatas];
        }

        return [.. RiseCore.ModuleManager.InternalModuleMetadatas.Where(x => !HasDisabledTag(x.Tags))];
    }

    private bool HasDisabledTag(string[] tags)
    {
        if (tags == null)
        {
            return false;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tagDisabled.Contains(tags[i]))
            {
                return true;
            }
        }
        return false;
    }

    private void Refresh(ModContainer container)
    {
        container.ClearAll();
        foreach (var mod in GetMods()) 
        {
            string title = mod.DisplayName?.ToUpperInvariant();
            if (string.IsNullOrEmpty(title))
            {
                title = mod.Name.ToUpperInvariant();
            }
            bool hasUpdate = RiseCore.UpdateChecks.HasUpdates.Contains(mod);

            if (hasUpdate)
            {
                title += "<t=variants/newVariantsTagSmall>";
            }
            container.AddMod(new () 
            {
                OutlineColor = ModContainer.DefaultOutlineColor,
                Title = title,
                Version = mod.Version.ToString().ToUpperInvariant(),
                Metadata = mod
            });
        }
    }
}


internal class ButtonBox : MenuItem
{
    private Point size;

    public Action OnConfirmed;
    public Action OnSelected;
    public static Color DefaultOutlineColor = new Color(132, 132, 132, 255);
    public static Color SelectedOutlineColor = Calc.HexToColor("D8F878");
    public static Color SelectedColor = SelectedOutlineColor;
    public static Color NotSelectedColor = Color.White;
    public Color OutlineColor;
    public Color TextColor;

    private Vector2 tweenFrom;
    private Vector2 tweenTo;

    public string Text => text;
    private string text;

    public ButtonBox(Rectangle rect, string text, Vector2 tweenFrom) : this(new Vector2(rect.X, rect.Y), new Point(rect.Width, rect.Height), text, tweenFrom) 
    {
    }

    public ButtonBox(Vector2 position, Point size, string text, Vector2 tweenFrom) : base(position)
    {
        this.size = size;
        OutlineColor = DefaultOutlineColor;
        this.tweenFrom = tweenFrom;
        this.tweenTo = position;
        this.text = text;
    }

    public override void Render()
    {
        base.Render();
        TextColor = Selected ? SelectedColor : NotSelectedColor;
        Draw.Rect(new Rectangle((int)Position.X, (int)Position.Y, size.X, size.Y), new Color(62, 62, 62, 255));
        Draw.TextCentered(TFGame.Font, text, new Vector2(Position.X + (size.X * 0.5f), Position.Y + (size.Y * 0.5f)), TextColor);
        Draw.HollowRect(new Rectangle((int)Position.X, (int)Position.Y, size.X, size.Y), OutlineColor);
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
        OnConfirmed?.Invoke();
    }

    protected override void OnDeselect()
    {
        OutlineColor = DefaultOutlineColor;
    }

    protected override void OnSelect()
    {
        OutlineColor = SelectedOutlineColor;
        OnSelected?.Invoke();
    }
}

internal class ModContainer : MenuItem
{
    public class Box 
    {
        public ModuleMetadata Metadata;
        public string Title;
        public string Version;
        public Color OutlineColor;
    }
    private int scrollYIndex = -1;
    private float scrollY;
    private List<Box> buttonBoxes = [];
    private Box currentSelected;
    private float tweenTo;
    private float tweenFrom;

    public Box CurrentSelected => currentSelected;
    public Action<ModuleMetadata> OnConfirmed;

    public static Color DefaultOutlineColor = new Color(132, 132, 132, 255);
    public static Color SelectedOutlineColor = Calc.HexToColor("D8F878");

    public ModContainer(Vector2 position) : base(position)
    {
    }

    public void AddMod(Box buttonBox) 
    {
        buttonBoxes.Add(buttonBox);
    }

    public void ClearAll()
    {
        scrollYIndex = -1;
        scrollY = 0;
        buttonBoxes.Clear();
    }

    public override void Added()
    {
        base.Added();
        tweenTo = Position.Y;
        tweenFrom = buttonBoxes.Count * 21;
        Position.Y += buttonBoxes.Count * 21;
    }

    [MonoModLinkTo("Monocle.Entity", "Update")]
    public void base_Update() {}

    public override void Update()
    {
        base_Update();

        if (!Selected) 
        {
            return;
        }

        if (LeftItem is not null && MenuInput.Left)
        {
            currentSelected.OutlineColor = DefaultOutlineColor;
            Selected = false;
            LeftItem.Selected = true;
            return;
        }

        if (MenuInput.Down)
        {
            if (currentSelected != null)
            {
                currentSelected.OutlineColor = DefaultOutlineColor;
            }
            scrollYIndex = Math.Min(buttonBoxes.Count - 1, scrollYIndex + 1);
            scrollY = Math.Max(0, scrollYIndex - 5) * 21;
            currentSelected = buttonBoxes[scrollYIndex];
        }
		else if (MenuInput.Confirm)
		{
			this.OnConfirm();
		}
        else if (MenuInput.Up)
        {
            if (scrollYIndex == 0)
            {
                scrollYIndex = -1;
                UpItem.Selected = true;
                Selected = false;
                currentSelected.OutlineColor = DefaultOutlineColor;
                return;
            }
            if (currentSelected != null)
            {
                currentSelected.OutlineColor = DefaultOutlineColor;
            }
            scrollYIndex = Math.Max(0, scrollYIndex - 1);
            scrollY = Math.Max(0, scrollYIndex - 5) * 21;
            currentSelected = buttonBoxes[scrollYIndex];
        }

        if (currentSelected != null)
        {
            currentSelected.OutlineColor = SelectedOutlineColor;
        }
    }

    public override void Render()
    {
        for (int i = 0; i < buttonBoxes.Count; i++)
        {
            var box = buttonBoxes[i];
            Vector2 position = Position; 
            position.Y += (i * 21) - scrollY;

            float num;

            if (position.Y < 60f)
            {
                num = (position.Y - 30f) / 18f;
                num = MathHelper.Clamp(num, 0f, 1f);
            }
            else
            {
                num = 1f - (position.Y - 220f) / 18f;
                num = MathHelper.Clamp(num, 0f, 1f);
            }

            Draw.Rect(new Rectangle((int)position.X, (int)position.Y, 128, 18), new Color(62, 62, 62, 255) * num);
            Draw.HollowRect(new Rectangle((int)position.X, (int)position.Y, 128, 18), box.OutlineColor * num);
            TextUtils.DrawIconText(TFGame.Font, box.Title, new Vector2(position.X + 2, position.Y + 4), Color.White * num);
            Draw.Text(TFGame.Font, box.Version, new Vector2(position.X + 2, position.Y + 10), Color.White * num);
        }
        base.Render();
    }

    public override void TweenIn()
    {
        Position.Y = tweenFrom;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 20, true);
        tween.OnUpdate = t =>
        {
            Position.Y = float.Lerp(tweenFrom, tweenTo, t.Eased);
        };
        Add(tween);
    }

    public override void TweenOut()
    {
        float start = Position.Y;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 12, true);
        tween.OnUpdate = t =>
        {
            Position.Y = float.Lerp(start, tweenFrom, t.Eased);
        };
        Add(tween);
    }

    protected override void OnConfirm()
    {
        OnConfirmed?.Invoke(currentSelected.Metadata);
    }

    protected override void OnDeselect()
    {
    }

    protected override void OnSelect()
    {
    }
}