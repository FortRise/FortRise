using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

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
            Add(val(e, e.Position()));
            return;
        }
        orig_LoadEntity(e);
    }

    [MonoModIgnore]
    [PreFixing("FortRise.RiseCore/Events", "System.Void Invoke_OnLevelEntered()", true)]
    public extern override void Begin();

    [MonoModIgnore]
    [PostFixing("FortRise.RiseCore/Events", "System.Void Invoke_OnLevelExited()", true)]
    public extern override void End();

    [MonoModIgnore]
    [PostFixing("TowerFall.Level", "System.Void DebugModeRender()")]
    public extern override void Render();
    

    public void DebugModeRender() 
    {
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