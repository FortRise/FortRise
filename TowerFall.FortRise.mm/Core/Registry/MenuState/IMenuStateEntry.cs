using TowerFall;

namespace FortRise;

public interface IMenuStateEntry
{
    public string Name { get; }
    public MenuStateConfiguration Configuration { get; init; }
    public MainMenu.MenuState MenuState { get; }
}
