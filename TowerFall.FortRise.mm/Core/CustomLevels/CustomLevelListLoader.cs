using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;
using TowerFall.Patching;

namespace FortRise;

public sealed class CustomLevelListLoader : Entity 
{
    private TowerFall.Patching.MapScene map;
    private List<MapButton> buttons;
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
        buttons = new List<MapButton>();

        if (map.Mode != MainMenu.RollcallModes.Trials)
        {
            buttons.Add(new CustomLevelCategoryButton(map.Mode));
        }
        
        spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.BackOut, 18, false);
        spinTween.OnUpdate = t => spin = MathHelper.Lerp(tweenStart, tweenEnd, t.Eased);
        Add(spinTween);
        Turn();

        Alarm.Set(this, 20, this.Turn, Alarm.AlarmMode.Looping);
        var start = this.Position = new Vector2(15f, 280f);
        var end = new Vector2(25f, 215f);
        introTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 4, true);

        var lockedLevels = new List<MapButton>();
        switch (map.Mode) 
        {
        case MainMenu.RollcallModes.DarkWorld: 
        {
            var set = map.GetLevelSet() ?? TowerRegistry.DarkWorldLevelSets[0];
            var currentLevel = TowerRegistry.DarkWorldTowerSets[set];
            map.SetLevelSet(set);
            for (int j = 0; j < currentLevel.Count; j++)
            {
                var mapButton = new TowerFall.DarkWorldMapButton(currentLevel[j]);
                if (mapButton.Locked)
                {
                    lockedLevels.Add(mapButton);
                    continue;
                }    
                buttons.Add(mapButton);
            }
        }
            break;
        case MainMenu.RollcallModes.Quest: 
        {
            var set = map.GetLevelSet() ?? TowerRegistry.QuestLevelSets[0];
            var currentLevel = TowerRegistry.QuestTowerSets[set];
            map.SetLevelSet(set);
            for (int j = 0; j < currentLevel.Count; j++)
            {
                var mapButton = new QuestMapButton(currentLevel[j]);
                if (mapButton.Locked)
                {
                    lockedLevels.Add(mapButton);
                    continue;
                }    
                buttons.Add(mapButton);
            }
        }
            break;
        case MainMenu.RollcallModes.Versus:
        {
            var set = map.GetLevelSet() ?? TowerRegistry.VersusLevelSets[0];
            var currentLevel = TowerRegistry.VersusTowerSets[set];
            map.SetLevelSet(set);
            for (int j = 0; j < currentLevel.Count; j++)
            {
                var mapButton = new TowerFall.VersusMapButton(currentLevel[j]);
                if (mapButton.Locked)
                {
                    lockedLevels.Add(mapButton);
                    continue;
                }    
                buttons.Add(mapButton);
            }
        }
            break;
        case MainMenu.RollcallModes.Trials:
        {
            var set = map.GetLevelSet() ?? TowerRegistry.TrialsLevelSet[0];
            var currentLevels = TowerRegistry.TrialsTowerSets[set];
            var list = new List<MapButton[]>();        
            var adv = new CustomLevelCategoryButton(map.Mode);
            buttons.Add(adv);
            list.Add(new MapButton[] { adv, adv, adv });
            if (currentLevels.Count == 0)
                break;

            // Y
            for (int i = 0; i < currentLevels.Count; i++) 
            {
                var array = new MapButton[currentLevels[0].Length];
                for (int j = 0; j < array.Length; j++) 
                {
                    buttons.Add(array[j] = new TowerFall.Patching.TrialsMapButton(currentLevels[i][j]));
                }
                for (int k = 0; k < array.Length; k++) 
                {
                    if (k > 0) 
                    {
                        array[k].UpButton = array[k - 1];
                    }
                    if (k < array.Length - 1) 
                    {
                        array[k].DownButton = array[k + 1];
                    }
                }
                list.Add(array);
            }

            // X
            for (int i = 0; i < list.Count; i++) 
            {
                if (i > 0) 
                {
                    for (int j = 0; j < list[i].Length; j++) 
                    {
                        list[i][j].LeftButton = list[i - 1][j];
                    }
                }
                if (i < list.Count - 1) 
                {
                    for (int j = 0; j < list[i].Length; j++) 
                    {
                        list[i][j].RightButton = list[i + 1][j];
                    }
                }
                for (int j = 0; j < list[i].Length; j++) 
                {
                    list[i][j].MapXIndex = i;
                }
            }
        }
            break;
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
        {
            return;
        }
        
        finished = true;
        if (buttons.Count > 0)
        {
            foreach (var button in buttons)
            {
                map.Buttons.Add(button);
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