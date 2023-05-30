using MonoMod;

namespace Monocle;

public class patch_Engine : Engine
{
    public Commands Commands { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw new System.Exception(value.ToString()); }
    
    public patch_Engine(int width, int height, float scale, string windowTitle) : base(width, height, scale, windowTitle)
    {
    }

    [MonoModLinkTo("Microsoft.Xna.Framework.Game", "System.Void Initialize()")]
    protected void base_Initialize() 
    {
        base.Initialize();
    }

    [MonoModReplace]
    protected override void Initialize() 
    {
        base_Initialize();
        this.Graphics.DeviceReset += this.OnGraphicsReset;
        this.Graphics.DeviceCreated += this.OnGraphicsCreated;
        patch_MInput.Initialize();
        this.Commands = new Commands();
    }
}