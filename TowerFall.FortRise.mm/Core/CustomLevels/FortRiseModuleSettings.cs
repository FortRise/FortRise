namespace FortRise;

internal sealed class FortRiseModuleSettings : ModuleSettings
{
    public bool OldIntroLogo { get; set; }

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Old Intro Logo", OldIntroLogo, (x) => OldIntroLogo = x);
    }
}