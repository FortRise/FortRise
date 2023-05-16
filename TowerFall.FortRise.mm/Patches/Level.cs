using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace TowerFall;

public class patch_Level : Level
{
    public static bool DebugMode;
    public patch_Level(Session session, XmlElement xml) : base(session, xml)
    {
    }

    public extern void orig_LoadEntity(XmlElement e);

    public void LoadEntity(XmlElement e) 
    {
        var name = e.Name;
        if (FortRise.RiseCore.LevelEntityLoader.TryGetValue(name, out var val)) 
        {
            Add(val(e));
            return;
        }
        orig_LoadEntity(e);
    }

    public extern void orig_Render();

    public override void Render()
    {
        orig_Render();
        if (DebugMode) 
        {
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Lerp(Matrix.Identity, Camera.Matrix, 1f));
            foreach (var entity in Layers[0].Entities) 
            {
                entity.DebugRender();
            }
			Draw.SpriteBatch.End();
        }
    }
}