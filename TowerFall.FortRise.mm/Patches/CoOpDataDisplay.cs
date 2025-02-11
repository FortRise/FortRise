using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_CoOpDataDisplay : CoOpDataDisplay
{
    private QuestButton quest;
    private DarkWorldButton darkWorld;
    private SineWave alphaSine;
    private Monolouge monolouge;
    private CoOpModeButton coOpModeButton;
    private bool tweeningOut;
    private float drawQuest;

    public patch_CoOpDataDisplay(QuestButton quest, DarkWorldButton darkWorld) : base(quest, darkWorld) {}
    public patch_CoOpDataDisplay(QuestButton quest, DarkWorldButton darkWorld, CoOpModeButton coOpModeButton) : base(quest, darkWorld) {}

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.MenuItem", "System.Void .ctor(Microsoft.Xna.Framework.Vector2)")]
    public void base_ctor(Vector2 position) {}

    [MonoModConstructor]
    public void ctor(QuestButton quest, DarkWorldButton darkWorld, CoOpModeButton coOpModeButton) 
    {
        base_ctor(Vector2.Zero);
        base.LayerIndex = -1;
        this.quest = quest;
        this.darkWorld = darkWorld;

        Add(alphaSine = new SineWave(120));
        this.coOpModeButton = coOpModeButton;
        this.coOpModeButton.ModeChanged += Changed;
        monolouge = this.coOpModeButton.CurrentMonolouge;
    }

    private void Changed(Monolouge monolouge) 
    {
        this.monolouge = monolouge;
    }

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.MenuItem", "System.Void Update()")]
    public void base_Update() {}

    public override void Update()
    {
        base_Update();
        if (tweeningOut)
        {
            drawQuest = Calc.Approach(drawQuest, 0f, 0.05f * Engine.TimeMult);
        }
        else 
        {
            drawQuest = Calc.Approach(drawQuest, 1f, 0.05f * Engine.TimeMult);
        }
    }

    [MonoModIgnore]
    [MonoModLinkTo("Monocle.Entity", "System.Void Render()")]
    public void base_Render() {}

    public override void Render()
    {
        base_Render();
        Vector2 move = Vector2.Lerp(Vector2.UnitX * -160f, Vector2.UnitX * 160f, Ease.CubeInOut(drawQuest));
        monolouge?.RenderBanner(this, move);
    }

    public override void Removed()
    {
        base.Removed();
        coOpModeButton.ModeChanged -= Changed;
    }

}

internal abstract class Monolouge 
{
    public abstract void AddElements(CoOpModeButton button);
    public abstract void RemoveElements(CoOpModeButton button);

    public abstract void Update();
    public abstract void Render();
    public abstract void RenderBanner(CoOpDataDisplay display, Vector2 offset);

    public abstract void OnAction(CoOpModeButton button);
}

internal class DarkWorldMonolouge : Monolouge 
{
    private static Color DarkWorldText = Calc.HexToColor("95F94D");
    private Sprite<string> icon;
    private Image glow;
    private SineWave glowSine;
    private string darkWhite;
    private string darkRed;
    private string darkGold;
    private Color darkWhiteColor;
    private Color darkRedColor;
    private Color darkGoldColor;

    public DarkWorldMonolouge() 
    {
        icon = TFGame.MenuSpriteData.GetSpriteString("DarkWorldIcon");
        icon.Play("selected");
        icon.Position = new Vector2(0, -10f);

        glow = new Image(TFGame.MenuAtlas["darkWorldModeGlow"], null);
        glow.Position = new Vector2(0, -10f);
        glow.CenterOrigin();

        glowSine = new SineWave(300);

        int darkWorldLength = GameData.DarkWorldTowers.Count;

        darkWhite = SaveData.Instance.DarkWorld.TotalWhiteSkulls.ToString();
        darkRed = SaveData.Instance.DarkWorld.TotalRedSkulls.ToString();
        darkGold = SaveData.Instance.DarkWorld.TotalGoldSkulls.ToString();
        darkWhiteColor = ((SaveData.Instance.DarkWorld.TotalWhiteSkulls >= darkWorldLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
        darkRedColor = ((SaveData.Instance.DarkWorld.TotalRedSkulls >= darkWorldLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
        darkGoldColor = ((SaveData.Instance.DarkWorld.TotalGoldSkulls >= darkWorldLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
    }

    public override void AddElements(CoOpModeButton button)
    {
        button.Add(icon);
        button.Add(glow);
        button.Add(glowSine);
    }

    public override void OnAction(CoOpModeButton button)
    {
        MainMenu.CurrentMatchSettings = MainMenu.DarkWorldMatchSettings;
        MainMenu.RollcallMode = MainMenu.RollcallModes.DarkWorld;
        button.MainMenu.State = MainMenu.MenuState.Rollcall;
    }

    public override void RemoveElements(CoOpModeButton button)
    {
        button.Remove(icon);
        button.Remove(glow);
        button.Remove(glowSine);
    }

    public override void Render()
    {
        icon.DrawOutline();
    }

    public override void RenderBanner(CoOpDataDisplay display, Vector2 offset)
    {
        Draw.TextureBannerV(TFGame.MenuAtlas["questResults/darkWorldBanner"], offset + new Vector2(0f, 185f), new Vector2(100f, 37f), Vector2.One, 0f, Color.White, SpriteEffects.None, display.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
        Draw.OutlineTextCentered(TFGame.Font, "FOR 1-4 ARCHERS", offset + new Vector2(0f, 160f), DarkWorldText, Color.Black);
        Draw.OutlineTextCentered(TFGame.Font, "ENTER THE DARK WORLD AND SEEK VENGEANCE!", offset + new Vector2(0f, 168f), Color.White, Color.Black);
        if (GameData.DarkWorldDLC)
        {
            Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigWhiteSkull"], offset + new Vector2(-40f, 190f), Color.White);
            Draw.OutlineTextCentered(TFGame.Font, this.darkWhite, offset + new Vector2(-40f, 208f), this.darkWhiteColor, 1f);
            Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigRedSkull"], offset + new Vector2(0f, 190f), Color.White);
            Draw.OutlineTextCentered(TFGame.Font, this.darkRed, offset + new Vector2(0f, 208f), this.darkRedColor, 1f);
            Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigGoldSkull"], offset + new Vector2(40f, 190f), Color.White);
            Draw.OutlineTextCentered(TFGame.Font, this.darkGold, offset + new Vector2(40f, 208f), this.darkGoldColor, 1f);
        }
        else
        {
            Draw.OutlineTextCentered(TFGame.Font, "REQUIRES", offset + new Vector2(0f, 183f), Color.White, Color.Black);
            Draw.TextureCentered(TFGame.MenuAtlas["darkWorld/smallGlow"], offset + new Vector2(0f, 195f), Color.White);
            Draw.TextureCentered(TFGame.MenuAtlas["darkWorld/small"], offset + new Vector2(0f, 195f), Color.White);
            Draw.OutlineTextCentered(TFGame.Font, "EXPANSION", offset + new Vector2(0f, 210f), QuestDifficultySelect.LegendaryColor, Color.Black);
        }
    }

    public override void Update()
    {
        glow.Color = Color.White * (0.8f * 1f + 0.2f * glowSine.Value * 1f);
    }
}

internal class QuestMonolouge : Monolouge 
{
    private Sprite<string> icon;
    private Image glow;
    private SineWave glowSine;
    private string questWhite;
    private string questRed;
    private string questGold;
    private Color questWhiteColor;
    private Color questRedColor;
    private Color questGoldColor;

    public QuestMonolouge() 
    {
        icon = TFGame.MenuSpriteData.GetSpriteString("QuestIcon");
        icon.Play("selected");
        icon.Position = new Vector2(0, -10f);

        glow = new Image(TFGame.MenuAtlas["questModeGlow"], null);
        glow.Position = new Vector2(0, -10f);
        glow.CenterOrigin();

        glowSine = new SineWave(300);

        int questLength = SaveData.Instance.Quest.Towers.Length;

        questWhite = SaveData.Instance.Quest.TotalWhiteSkulls.ToString();
        questRed = SaveData.Instance.Quest.TotalRedSkulls.ToString();
        questGold = SaveData.Instance.Quest.TotalGoldSkulls.ToString();
        questWhiteColor = ((SaveData.Instance.Quest.TotalWhiteSkulls >= questLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
        questRedColor = ((SaveData.Instance.Quest.TotalRedSkulls >= questLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
        questGoldColor = ((SaveData.Instance.Quest.TotalGoldSkulls >= questLength) ? QuestDifficultySelect.LegendaryColor : Color.White);
    }

    public override void AddElements(CoOpModeButton button)
    {
        button.Add(icon);
        button.Add(glow);
        button.Add(glowSine);
    }

    public override void OnAction(CoOpModeButton button)
    {
        MainMenu.CurrentMatchSettings = MainMenu.QuestMatchSettings;
        MainMenu.RollcallMode = MainMenu.RollcallModes.Quest;
        button.MainMenu.State = MainMenu.MenuState.Rollcall;
    }

    public override void RemoveElements(CoOpModeButton button)
    {
        button.Remove(icon);
        button.Remove(glow);
        button.Remove(glowSine);
    }

    public override void Render()
    {
        icon.DrawOutline();
    }

    public override void RenderBanner(CoOpDataDisplay display, Vector2 offset)
    {
        Draw.TextureBannerV(TFGame.MenuAtlas["questResults/tipBanner"], offset + new Vector2(0, 185f), new Vector2(100f, 37f), Vector2.One, 0f, Color.White, SpriteEffects.None, display.Scene.FrameCounter * 0.03f, 4f, 5, 0.3926991f);
        Draw.OutlineTextCentered(TFGame.Font, "FOR 1 OR 2 ARCHERS", offset + new Vector2(0, 160f), QuestDifficultySelect.LegendaryColor, Color.Black);
        Draw.OutlineTextCentered(TFGame.Font, "DEFEND TOWERFALL FROM INVADING MONSTERS!", offset + new Vector2(0, 168f), Color.White, Color.Black);
        Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigWhiteSkull"], offset + new Vector2(-40f, 190f), Color.White);
        Draw.OutlineTextCentered(TFGame.Font, this.questWhite, offset + new Vector2(-40f, 208f), this.questWhiteColor, 1f);
        Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigRedSkull"], offset + new Vector2(0, 190f), Color.White);
        Draw.OutlineTextCentered(TFGame.Font, this.questRed, offset + new Vector2(0f, 208f), this.questRedColor, 1f);
        Draw.TextureCentered(TFGame.MenuAtlas["questResults/bigGoldSkull"], offset + new Vector2(40f, 190f), Color.White);
        Draw.OutlineTextCentered(TFGame.Font, this.questGold, offset + new Vector2(40f, 208f), this.questGoldColor, 1f);
    }

    public override void Update()
    {
        glow.Color = Color.White * (0.8f * 1f + 0.2f * glowSine.Value * 1f);
    }
}

public class CoOpModeButton : BorderButton
{
    private const int QuestMode = 0;
    private const int DarkWorldMode = 1;
    private Wiggler iconWiggler;
    private int[] modeID;
    private int currentMode;
    private Monolouge[] monolouges = [new QuestMonolouge(), new DarkWorldMonolouge()];
    private Monolouge monolouge;

    internal event Action<Monolouge> ModeChanged;
    internal Monolouge CurrentMonolouge => monolouge;


    public CoOpModeButton(Vector2 position, Vector2 tweenFrom) : base(position, tweenFrom, 200, 30)
    {
        // TODO do not hardcode this
        modeID = [QuestMode, DarkWorldMode];
        UpdateSides();
        iconWiggler = Wiggler.Create(15, 6f);
        Add(iconWiggler);
    
        UpdateMode();
    }

    public override void Update()
    {
        base.Update();

        monolouge.Update();

        if (Selected)
        {
            if (MenuInput.Right)
            {
                if (currentMode != modeID.Length - 1)
                {
                    currentMode++;

                    Sounds.ui_move2.Play();
                    iconWiggler.Start();
                    UpdateSides();
                    OnConfirm();
                    UpdateMode();
                }
            }
            else if (MenuInput.Left)
            {
                if (currentMode != 0)
                {
                    currentMode--;

                    Sounds.ui_move2.Play();
                    iconWiggler.Start();
                    UpdateSides();
                    OnConfirm();
                    UpdateMode();
                }
            }
            else if (MenuInput.Confirm)
            {
                monolouge.OnAction(this);
            }
        }
    }

    private void UpdateMode()
    {
        monolouge?.RemoveElements(this);
        monolouge = monolouges[currentMode];
        monolouge.AddElements(this);
        ModeChanged?.Invoke(monolouge);
    }

    public override void Render()
    {
        monolouge.Render();
        base.Render();
        // Draw.OutlineTextureCentered(
        //     GetModeIcon(), 
        //     Position + new Vector2(0, -20f), 
        //     Color.White, 
        //     new Vector2(1f + iconWiggler.Value * 0.1f, 1f - iconWiggler.Value * 0.1f)
        // );
        Draw.OutlineTextCentered(TFGame.Font, GetModeName(), Position + new Vector2(0f, 14f), DrawColor, 2f);
    }

    private string GetModeName() 
    {
        return currentMode switch {
            1 => "DARKWORLD",
            0 => "QUEST",
            _ => "UNKNOWN"
        };
    }

    private void UpdateSides() 
    {
        DrawLeft = currentMode > 0;
        DrawRight = currentMode < modeID.Length - 1;
    }
}