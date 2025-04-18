#pragma warning disable CS0649
namespace FortRise.ImGuiLib;

internal sealed class ImGuiSettings : ModuleSettings
{
    [SettingsName("Debug Window Enabled")]
    public bool IsEnabled;
}