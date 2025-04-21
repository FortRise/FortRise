using TowerFall;

namespace FortRise;

internal sealed class MenuStateEntry(string name, MenuStateConfiguration configuration, MainMenu.MenuState menuState) : IMenuStateEntry
{
    public string Name { get; init; } = name;
    public MenuStateConfiguration Configuration { get; init; } = configuration;
    public MainMenu.MenuState MenuState { get; init; } = menuState;
}
