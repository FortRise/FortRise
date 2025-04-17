using Monocle;
using TowerFall;

namespace FortRise;

public class ModVariantPreset : VariantButton
{
    private Variant[] variants;

    public ModVariantPreset(Subtexture icon, string title, string description, Variant[] variants) : base(icon, title, description)
    {
        this.variants = variants;
    }

    protected override void OnConfirm()
    {
        base.OnConfirm();
        Sounds.ui_click.Play(160f, 1f);
        // disable all variants first 
        foreach (var allVariant in MainMenu.VersusMatchSettings.Variants.Variants)
        {
            allVariant.Value = false;
        }

        // then enable what matters
        foreach (var variant in variants)
        {
            variant.Value = true;
        }
    }

    protected override void OnSelect()
    {
        base.OnSelect();
        MainMenu.ButtonGuideC.Clear();
        MainMenu.ButtonGuideD.Clear();
    }

    public override void Render()
    {
        base.Render();
        DrawSelection(NormalSelection);
        DrawBubble();
    }
}