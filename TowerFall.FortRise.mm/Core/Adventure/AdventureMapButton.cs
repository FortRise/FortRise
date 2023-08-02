using System;
using System.Collections.Generic;
using System.Text;
using FortRise;
using FortRise.Adventure;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using TeuJson;
using TowerFall;

namespace FortRise.Adventure;

public enum AdventureType 
{
    Quest,
    DarkWorld,
    Trials,
    Versus
}

public sealed class AdventureMapButton : MapButton
{
    // Quite needed to do this to access the private set
    // I filled it with some non sense so the backing field won't generate
    [MonoModIgnore]
    public new string Author { get => null; set => throw new System.Exception(value); }
    private string author;
    private float lockedMessageLerp;
    private string lockedTextA;
    private string lockedTextB;
    private ModuleMetadata[] requiredMods;
    private SineWave lockedSine;
    private bool wasSelected;
    private AdventureType type;


    public AdventureMapButton(AdventureWorldTowerData data, AdventureType type) : base(new TowerMapData(data))
    {
        this.type = type;
        author = data.Author.ToUpperInvariant();
        if (string.IsNullOrEmpty(data.RequiredMods))   
            return;
        try 
        {
            requiredMods = JsonTextReader.FromText(data.RequiredMods).ConvertToArray<ModuleMetadata>();
            string currentRequired = null;
            int more = 0;
            foreach (var mod in requiredMods) 
            {
                if (FortRise.RiseCore.InternalModuleMetadatas.Contains(mod))
                    continue;
                
                currentRequired = mod.Name;
                more++;
            }
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(currentRequired)) 
            {
                lockedTextA = "REQUIRED MODS"; 
                Locked = true;
                sb.Append(currentRequired);
                if (more > 1) 
                {
                    sb.Append(" ");
                    sb.Append($"and {more - 1} more..");
                }

                lockedTextB = sb.ToString().ToUpperInvariant();
            }
        }
        catch (Exception e)
        {
            lockedTextA = "ERROR PARSING";
            lockedTextB = "SOMETHING WENT WRONG PARSING THE REQUIRED METADATA";
            Locked = true;
            Logger.Error("Something went wrong parsing the required Metadata");
            Logger.Error(e.ToString());
        }
    }
    public override void Added()
    {
        Author = author;
        base.Added();

        if (Locked)
        {
            var smallLock = new Image(TFGame.MenuAtlas["map/smallLock"]);
            smallLock.CenterOrigin();
            smallLock.Position = new Vector2(0f, -14f);
            Add(smallLock);
            lockedSine = new SineWave(120);
            Add(lockedSine);
        }
    }

    protected override bool GetLocked()
    {
        return false;
    }

    public override void Update()
    {
        base.Update();
        if (Locked) 
        {
            if (Selected) 
            {
                lockedMessageLerp = Calc.Approach(lockedMessageLerp, 1f, 0.1f * Engine.TimeMult);
            }
            else 
            {
                lockedMessageLerp = Calc.Approach(lockedMessageLerp, 0f, 0.2f * Engine.TimeMult);
            }
        }
        wasSelected = Selected;
    }

    public override void Render()
    {
        base.Render();
        if (lockedMessageLerp > 0f)
        {
            Subtexture lockedWindow = TFGame.MenuAtlas["map/lockedWindow"];
            var pos = Vector2.Lerp(Position + new Vector2(0f, -10f), 
                Position + new Vector2(0f, -36f), Ease.CubeOut(lockedMessageLerp));
            pos += Vector2.UnitY * this.lockedSine.Value * 3f;
            Draw.TextureCentered(lockedWindow, pos, Color.White * lockedMessageLerp);
            Draw.TextCentered(TFGame.Font, lockedTextA, pos + new Vector2(0f, -8f), Color.Black * lockedMessageLerp);
            Draw.OutlineTextCentered(TFGame.Font, lockedTextB, pos + new Vector2(0f, 0f), Color.Black * lockedMessageLerp, Color.White * lockedMessageLerp);
        }
    }

    public override void OnConfirm()
    {
        MainMenu.DarkWorldMatchSettings.LevelSystem = base.Data.GetLevelSystem();
        base.Map.TweenOutButtons();
        base.Map.Add<DarkWorldDifficultySelect>(new DarkWorldDifficultySelect());
    }

    protected override List<Image> InitImages()
    {
        return patch_MapButton.InitAdventureWorldGraphics(Data.ID.X);
    }
}