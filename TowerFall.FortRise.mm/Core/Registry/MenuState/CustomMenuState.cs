using TowerFall;

namespace FortRise;

public abstract class CustomMenuState 
{
    public MainMenu Main { get; set; }


    public CustomMenuState(MainMenu main)
    {
        Main = main;
    }

    public abstract void Create();
    public abstract void Destroy();
}
