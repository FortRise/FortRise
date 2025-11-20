using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_MapRenderer : MapRenderer
{
    private MapRendererData data;
    private Sprite<string> twilightSpire;
    private Sprite<string> sunkenCity;
    private Sprite<string> towerForge;
    private Sprite<string> ascension;
    private Sprite<string> theAmaranth;
    private Sprite<string> dreadwood;
    private Sprite<string> darkfang;
    private Sprite<string> cataclysm;

    private Vector2 shakeOffset;
    private List<GraphicsComponent> graphics;

    // The devs does not make these properties instance instead of static.
    // Fine, I'll do it myself.
    public int GetInstanceWidth() 
    {
        if (data != null) 
        {
            return (int)data.Size.X;
        }
        return Width;
    }

    public int GetInstanceHeight() 
    {
        if (data != null) 
        {
            return (int)data.Size.Y;
        }
        return Height;
    }

    public Vector2 GetInstanceCenter() 
    {
        if (data != null) 
        {
            return data.Size * 0.5f;
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
        if (data is not null)
        {
            data.StartSelection(towerName);
            return;
        }

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
            if (data is not null)
            {
                data.StartSelection(towerName);
            }
            break;
        }
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

        if (data != null && data.Water != null)
        {
            Draw.SineTextureV(
                data.Water, 
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
        if (data != null) 
        {
            data.Deselection();
            Remove(data);
            data = null;
        }

        if (levelSet == null || levelSet == "TowerFall")
        {
            ToggleAllVanillaElements(true);
            return;
        }

        var entry = MapRendererRegistry.GetEntryFromLevelSet(levelSet);
        if (entry is not null)
        {
            var mapRenderer = new MapRendererData(entry.Configuration);
            data = mapRenderer;
            Add(data); 

            ToggleAllVanillaElements(!entry.Configuration.HideVanillaElements);
            return;
        }

        ToggleAllVanillaElements(true);
    }

    public void ToggleAllVanillaElements(bool toggle) 
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
