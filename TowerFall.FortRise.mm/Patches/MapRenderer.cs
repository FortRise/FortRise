using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using TowerFall.Patching;

namespace TowerFall;

public class patch_MapRenderer : MapRenderer
{
    private MapRendererData node;
    private Sprite<string> twilightSpire;
    private Sprite<string> sunkenCity;
    private Sprite<string> towerForge;
    private Sprite<string> ascension;
    private Sprite<string> theAmaranth;
    private Sprite<string> dreadwood;
    private Sprite<string> darkfang;
    private Sprite<string> cataclysm;
    private string currentTowerID;

    private Vector2 shakeOffset;
    private List<GraphicsComponent> graphics;

    // The devs does not make these properties instance instead of static.
    // Fine, I'll do it myself.
    public int GetInstanceWidth() 
    {
        if (node != null && node.Land != null) 
        {
            return node.Land.Width;
        }
        return Width;
    }

    public int GetInstanceHeight() 
    {
        if (node != null && node.Land != null) 
        {
            return node.Land.Height;
        }
        return Height;
    }

    public Vector2 GetInstanceCenter() 
    {
        if (node != null && node.Land != null) 
        {
            return node.Land.HalfSize;
        }
        return Center;
    }


    public patch_MapRenderer(bool forceMoonstone) : base(forceMoonstone)
    {
    }

    public extern void orig_OnStartSelection(string towerName);

    [MonoModReplace]
    public void OnSelectionChange(string towerName) 
    {
        var scene = Scene as MapScene;
        var levelSet = scene.GetLevelSet();
        //if (levelSet == "TowerFall") 
        //{
        //    orig_OnSelectionChange(towerName);
        //    return;
        //}

        if (ExtendedGameData.InternalMapRenderers.TryGetValue(levelSet, out var node)) 
        {
            node.StartSelection(towerName);
        }

        currentTowerID = towerName;

        if (towerName == VanillaConstants.Towers.TwilightSpire)
        {
            SelectTwilightSpire();
        }
        else
        {
            DeselectTwilightSpire();
        }

        if (towerName == VanillaConstants.Towers.SunkenCity)
        {
            RevealSunkenCity();
        }
        else
        {
            HideSunkenCity();
        }

        if (towerName == VanillaConstants.Towers.Towerforge)
        {
            SelectTowerForge();
        }
        else
        {
            DeselectTowerForge();
        }

        if (towerName == VanillaConstants.Towers.Ascension)
        {
            SelectAscension();
        }
        else
        {
            DeselectAscension();
        }

        if (towerName == VanillaConstants.Towers.TheAmaranth)
        {
            SelectTheAmaranth();
        }
        else
        {
            DeselectTheAmaranth();
        }

        if (towerName == VanillaConstants.Towers.Dreadwood)
        {
            SelectDreadwood();
        }
        else
        {
            DeselectDreadwood();
        }

        if (towerName == VanillaConstants.Towers.Darkfang)
        {
            SelectDarkfang();
        }
        else
        {
            DeselectDarkfang();
        }

        if (towerName == VanillaConstants.Towers.Cataclysm || towerName == VanillaConstants.Towers.DarkGauntlet)
        {
            SelectCataclysm();
        }
        else
        {
            DeselectCataclysm();
        }
    }

    [MonoModReplace]
    public void OnStartSelection(string towerName)
    {
        switch (towerName)
        {
        case VanillaConstants.Towers.TwilightSpire:
            StartTwilightSpire();
            break;

        case VanillaConstants.Towers.SunkenCity:
            StartSunkenCity();
            break;

        case VanillaConstants.Towers.Towerforge:
            StartTowerForge();
            break;

        case VanillaConstants.Towers.Ascension:
            StartAscension();
            break;

        case VanillaConstants.Towers.TheAmaranth:
            StartTheAmaranth();
            break;

        case VanillaConstants.Towers.Dreadwood:
            StartDreadwood();
            break;

        case VanillaConstants.Towers.Darkfang:
            StartDarkfang();
            break;

        case VanillaConstants.Towers.Cataclysm:
        case VanillaConstants.Towers.DarkGauntlet:
            StartCataclysm();
            break;
        default:
            break;
        }
        
        currentTowerID = towerName;
    }
    

    [MonoModLinkTo("Monocle.CompositeComponent", "System.Void Render()")]
    [MonoModIgnore]
    public void base_Render() { base.Render(); }


    [MonoModReplace]
    public override void Render()
    {
        var mapOffset = Calc.Round(Offset + shakeOffset - Origin);
        foreach (GraphicsComponent graphicsComponent in graphics)
        {
            graphicsComponent.Position += mapOffset;
        }

        if (node != null && node.Water != null)
        {
            Draw.SineTextureV(
                node.Water, 
                Entity.Position + mapOffset, 
                new Vector2(5f, 0f), 
                Vector2.One, 0f, 
                Color.White, 
                SpriteEffects.None, 
                Scene.FrameCounter * 0.03f, 
                sliceSize: 1, 
                sliceAdd: 22.50f * Calc.DEG_TO_RAD
            );
        }
        else 
        {
            Draw.SineTextureV(
                TFGame.MenuAtlas["mapWater"], 
                Entity.Position + mapOffset, 
                new Vector2(5f, 0f), 
                Vector2.One, 0f, 
                Color.White, 
                SpriteEffects.None, 
                Scene.FrameCounter * 0.03f, 
                sliceSize: 1, 
                sliceAdd: 22.50f * Calc.DEG_TO_RAD
            );
        }

        base_Render();

        if (theAmaranth.CurrentFrame > 0)
        {
            theAmaranth.DrawOutline(1);
            theAmaranth.Render();
        }
        if (dreadwood.CurrentFrame > 0)
        {
            dreadwood.DrawOutline(1);
            dreadwood.Render();
        }
        if (darkfang.CurrentFrame > 0)
        {
            darkfang.DrawOutline(1);
            darkfang.Render();
        }
        if (cataclysm.CurrentFrame > 0)
        {
            cataclysm.DrawOutline(1);
            cataclysm.Render();
        }

        foreach (GraphicsComponent graphicsComponent in graphics)
        {
            graphicsComponent.Position -= mapOffset;
        }
    }

    public void ChangeLevelSet(string levelSet) 
    {
        if (node != null) 
        {
            node.Deselection();
            Remove(node);
            node = null;
        }

        if (levelSet == null || levelSet == "TowerFall") 
        {
            ToggleAllMainElements(true);
            return;
        }

        if (ExtendedGameData.InternalMapRenderers.TryGetValue(levelSet, out var val)) 
        {
            node = val;
            Add(node); 
            ToggleAllMainElements(false);
            return;
        }

        ToggleAllMainElements(true);
    }

    public void ToggleAllMainElements(bool toggle) 
    {
        twilightSpire.Visible = toggle;
        sunkenCity.Visible = toggle;
        towerForge.Visible = toggle;
        ascension.Visible = toggle;
        theAmaranth.Visible = toggle;
        dreadwood.Visible = toggle;
        darkfang.Visible = toggle;
        cataclysm.Visible = toggle;
    }

    [MonoModIgnore]
    private extern void SelectTwilightSpire();

    [MonoModIgnore]
    private extern void DeselectTwilightSpire();

    [MonoModIgnore]
    private extern void SelectTowerForge();

    [MonoModIgnore]
    private extern void HideSunkenCity();

    [MonoModIgnore]
    private extern void DeselectTowerForge();

    [MonoModIgnore]
    private extern void SelectAscension();

    [MonoModIgnore]
    private extern void DeselectAscension();

    [MonoModIgnore]
    private extern void DeselectTheAmaranth();

    [MonoModIgnore]
    private extern void DeselectDreadwood();

    [MonoModIgnore]
    private extern void DeselectDarkfang();

    [MonoModIgnore]
    private extern void DeselectCataclysm();

    [MonoModIgnore]
    private extern void StartTwilightSpire();

    [MonoModIgnore]
    private extern void StartSunkenCity();

    [MonoModIgnore]
    private extern void StartTowerForge();

    [MonoModIgnore]
    private extern void StartAscension();

    [MonoModIgnore]
    private extern void StartTheAmaranth();

    [MonoModIgnore]
    private extern void StartDreadwood();

    [MonoModIgnore]
    private extern void StartDarkfang();

    [MonoModIgnore]
    private extern void StartCataclysm();

}
