namespace FortRise.ImGuiLib;

internal sealed class ApiImplementation : IFortRiseImGuiAPI
{
    public ApiImplementation()
    {
    }

    public void RegisterTab(IFortRiseImGuiAPI.ITabItem tab)
    {
        TabItemManager.Instance.Register(tab);
    }

    public void UnregisterTab(IFortRiseImGuiAPI.ITabItem tab)
    {
        TabItemManager.Instance.Unregister(tab);
    }
}
