using System.Collections.Generic;

namespace FortRise.ImGuiLib;

internal sealed class TabItemManager 
{
    public static TabItemManager Instance { get; private set; } = new TabItemManager();

    private List<IFortRiseImGuiAPI.ITabItem> tabs = new List<IFortRiseImGuiAPI.ITabItem>();
    public IReadOnlyList<IFortRiseImGuiAPI.ITabItem> Tabs => tabs;

    public TabItemManager()
    {
        Instance = this;
    }

    public void Register(IFortRiseImGuiAPI.ITabItem tab)
    {
        tabs.Add(tab);
    }

    public void Unregister(IFortRiseImGuiAPI.ITabItem tab)
    {
        tabs.Remove(tab);
    }
}