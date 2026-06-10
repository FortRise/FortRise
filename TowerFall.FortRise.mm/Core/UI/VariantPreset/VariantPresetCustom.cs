using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TowerFall;

namespace FortRise;

public class VariantPresetCustom : VariantButton
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "scaleWiggler")]
    private static extern ref Wiggler scaleWiggler(VariantButton target);


    private CustomVariantPreset preset;
    private patch_MatchVariants variants;
    private Color color;
    private int id;

    public VariantPresetCustom(Subtexture subtexture, CustomVariantPreset preset, patch_MatchVariants variants, int id) : base(subtexture, preset.Name, "")
    {
        this.variants = variants;
        this.preset = preset;
        this.id = id;

        color = Calc.HexToColor(preset.Color);
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        if (variants.ApplyPreset(new VariantPresetData() { Variants = [.. FortRiseModule.SaveData.VariantPresets[id].Variants] }))
        {
            Sounds.ui_click.Play(160f);
        }
        else if (!TFGame.OpenStoreDarkWorldDLC())
        {
            Sounds.ui_clickBack.Play(160f);
        }
    }

    public override void Update()
    {
        if (Selected && MenuInput.Alt)
        {
            FortRiseModule.SaveData.VariantPresets.Remove(preset);
            variants.Recalculate((patch_MainMenu)MainMenu);

            Saver saver = new Saver(true, () => saver = null);
            saver.CanHandleError = true;
            Scene.Add(saver);
        }

        base.Update();
    }

    [MonoModLinkTo("Monocle.Entity", "Render")]
    [MonoModIgnore]
    public void base_Render() { }

    public override void Render()
    {
        Image.Color = color * Alpha;
        Image.Scale = Vector2.One * (1f + scaleWiggler(this).Value * 0.2f) * (Selected ? 1.2f : 1f);
        if (Alpha >= 1f)
        {
            Image.DrawOutline(1);
        }

        base_Render();

        DrawSelection(NormalSelection);
        DrawBubble();
    }
}
