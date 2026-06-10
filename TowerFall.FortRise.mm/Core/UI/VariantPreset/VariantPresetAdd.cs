using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class VariantPresetAdd : VariantButton
{
    private patch_MatchVariants variants;

    public VariantPresetAdd(patch_MatchVariants variants) : base(FortRiseModule.PresetAddIcon, "ADD PRESET", string.Empty)
    {
        this.variants = variants;
    }

    // TODO: create a virtual keyboard input for controller users. hhh
    protected override void OnConfirm()
    {
        Selected = false;

        UIVariantAddPreset variantAddPreset = new UIVariantAddPreset(Vector2.Zero, (text, color) =>
        {
            var variantPresetData = variants.GetPreset();
            FortRiseModule.SaveData.VariantPresets.Add(new CustomVariantPreset()
            {
                Name = text,
                Color = color.ColorToRGBHex,
                Variants = [.. variantPresetData.Variants]
            });

            Sounds.ui_click.Play(160f);

            variants.Recalculate((patch_MainMenu)MainMenu);
            Saver saver = new Saver(true, () => saver = null);
            saver.CanHandleError = true;
            Scene.Add(saver);
        });
        Scene.Add(variantAddPreset);

        Sounds.ui_click.Play(160f);

        base.OnConfirm();
    }

    public override void Render()
    {
        base.Render();
        DrawSelection(NormalSelection);
        DrawBubble();
    }
}
