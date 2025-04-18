using System;
using Microsoft.Xna.Framework.Graphics;

namespace FortRise.ImGuiLib;

public interface IFortRiseImGuiAPI
{
    public interface IItem;

    void RegisterTab(ITabItem tab);
    void UnregisterTab(ITabItem tab);

    public interface ITabItem : IItem
    {
        string Title { get; }
        void Render(IRenderer renderer);
    }

    public interface IRenderer 
    {
        IntPtr BindTexture(Texture2D texture);
        void UnbindTexture(IntPtr textureId);
    }
}