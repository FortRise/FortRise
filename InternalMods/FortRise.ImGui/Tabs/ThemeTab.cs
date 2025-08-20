using ImGuiNET;

namespace FortRise.ImGuiLib;


internal sealed class ThemeTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Themes";
    private string searchBar = "";

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        ImGui.InputText("Search", ref searchBar, 100);
    }
}
