using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall;

public sealed class AdventureListLoader : Entity 
{
    private MapScene map;
    private List<MapButton> buttons;
    private float spin;
    private Tween spinTween;
    private float tweenStart;
    private float tweenEnd;
    private bool finished;
    private Tween introTween;

    public AdventureListLoader(MapScene map) : base(0) 
    {
        this.map = map;
        Depth = -100000;
        Visible = false;
        buttons = new List<MapButton>();
        buttons.Add(new UploadMapButton());
        spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.BackOut, 18, false);
        spinTween.OnUpdate = t => spin = MathHelper.Lerp(tweenStart, tweenEnd, t.Eased);
        Add(spinTween);
        Turn();

        Alarm.Set(this, 20, this.Turn, Alarm.AlarmMode.Looping);
        var start = this.Position = new Vector2(15f, 280f);
        var end = new Vector2(25f, 215f);
        introTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 4, true);

        var lockedLevels = new List<MapButton>();
        for (int j = 0; j < patch_GameData.AdventureWorldTowers.Count; j++)
        {
            var mapButton = new AdventureMapButton(patch_GameData.AdventureWorldTowers[j]);
            if (mapButton.Locked)
            {
                lockedLevels.Add(mapButton);
                continue;
            }    
            buttons.Add(mapButton);
        }

        foreach (MapButton lockedLevel in lockedLevels) 
        {
            buttons.Add(lockedLevel);
        }
    }

    public override void Update()
    {
        base.Update();
        if (finished)
            return;
        
        finished = true;
        if (buttons.Count > 0)
        {
            foreach (var button in buttons)
            {
                map.Buttons.Add(button);
                map.Add(button);
            }
            map.LinkButtonsList();
            map.InitButtons(map.Buttons[0]);
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