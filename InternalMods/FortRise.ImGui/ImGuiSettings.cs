#pragma warning disable CS0649
namespace FortRise.ImGuiLib;

internal sealed class ImGuiSettings : ModuleSettings
{
    public bool IsEnabled { get; set; }

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Debug Window Enabled", IsEnabled, (x) => IsEnabled = x);
    }
}