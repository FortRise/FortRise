using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_Level : Level
{
    public XmlElement XML
    {
        [MonoModIgnore]
        get => throw null;
        [MonoModIgnore]
        private set => throw null;
    }
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
            Add(val(e, e.Position(), e.Nodes()));
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
        if (patch_TFCommands.CheatMode) 
        {
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Lerp(Matrix.Identity, Camera.Matrix, 1f));
            Draw.OutlineTextJustify(TFGame.Font, "CHEAT ACTIVATED", new Vector2(0, 0), Color.White, Color.Black, new Vector2(0, 0), 1f);
			Draw.SpriteBatch.End();
        }
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

    public void Reload(XmlElement xml, int width, int height) 
    {
        XML = xml;
        bool[,] solidsBitData = Calc.GetBitData(this.XML["Solids"].InnerText, width, height);
        bool[,] bgBitData = Calc.GetBitData(this.XML["BG"].InnerText, width, height);
        int[,] overwriteData = Calc.ReadCSVIntGrid(this.XML["BGTiles"].InnerText, width, height);
        foreach (var entity in Layers[0].Entities) 
        {
            if (entity is LevelEntity and not (TowerFall.Player or Enemy or TreasureChest or Arrow))
                continue;
            
            Remove(entity);
        }

        Add<LevelTiles>(Tiles = new LevelTiles(this.XML, solidsBitData));
        Add<LevelBGTiles>(BGTiles = new LevelBGTiles(this.XML, bgBitData, solidsBitData, overwriteData));
        foreach (XmlElement xmlElement2 in this.XML["Entities"])
        {
            LoadEntity(xmlElement2);
        }
        if (xml["Entities"].GetElementsByTagName("BlueSwitchBlock").Count > 0 || xml["Entities"].GetElementsByTagName("RedSwitchBlock").Count > 0)
        {
            Add<SwitchBlockControl>(new SwitchBlockControl(this.Session));
        }
        else if (this.Session.MatchSettings.Variants.DarkPortals)
        {
            Add<DarkPortalsVariantSequence>(new DarkPortalsVariantSequence());
        }
    }
}