using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public sealed class CustomLevelListLoader : Entity 
{
    private TowerFall.Patching.MapScene map;
    private float spin;
    private Tween spinTween;
    private float tweenStart;
    private float tweenEnd;
    private bool finished;
    private Tween introTween;
    private int startingID;

    public CustomLevelListLoader(TowerFall.Patching.MapScene map, int id) : base(0) 
    {
        startingID = id;
        this.map = map;
        Depth = -100000;
        Visible = false;

        spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.BackOut, 18, false);
        spinTween.OnUpdate = t => spin = MathHelper.Lerp(tweenStart, tweenEnd, t.Eased);
        Add(spinTween);
        Turn();

        Alarm.Set(this, 20, this.Turn, Alarm.AlarmMode.Looping);
        var start = this.Position = new Vector2(15f, 280f);
        var end = new Vector2(25f, 215f);
        introTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 4, true);

        switch (map.Mode)
        {
            case MainMenu.RollcallModes.DarkWorld:
                map.InitDarkWorldButtons();
                break;
            case MainMenu.RollcallModes.Quest:
                map.InitQuestButtons();
                break;
            case MainMenu.RollcallModes.Versus:
                map.InitVersusButtons();
                break;
            case MainMenu.RollcallModes.Trials:
                map.InitTrialsButtons();
                break;
        } 
    }

    public override void Update()
    {
        base.Update();
        if (finished)
        {
            return;
        }

        var buttons = map.Buttons;
        finished = true;
        if (buttons.Count > 0)
        {
            foreach (var button in buttons)
            {
                map.Add(button);
            }
            if (startingID >= map.Buttons.Count) 
            {
                startingID = map.Buttons.Count - 1;
            }
            if (map.Mode != MainMenu.RollcallModes.Trials)
            {
                map.LinkButtonsList();
            }

            map.InitButtons(map.Buttons[startingID]);
            map.ScrollToButton(map.Selection);
            introTween.Stop();
            var start = Position;
            var end = new Vector2(15f, 280f);
            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 10, true);
            tween.OnUpdate = t =>
            {
                Position = Vector2.Lerp(start, end, t.Eased);
            };
            tween.OnComplete = t =>
            {
                RemoveSelf();
            };
            Add(tween);
        }
        spin += 0.19634955f * Engine.TimeMult;
    }

    private void Turn() 
    {
        const float spinValue = 360 * Calc.DEG_TO_RAD;
        spin = (spin + spinValue) % spinValue;
        tweenStart = spin;
        tweenEnd = spin + (45 * Calc.DEG_TO_RAD);
        spinTween.Start();
    }

    public override void Render()
    {
        base.Render();
        Draw.OutlineTextureCentered(TFGame.MenuAtlas["workshopButton"], this.Position, this.spin);
    }
}