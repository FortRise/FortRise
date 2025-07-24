using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_WorkshopListLoader : WorkshopListLoader
{
    private MapScene map;
    private List<MapButton> buttons;
    private Tween spinTween;
    private Tween introTween;
    private float spin;
    private float tweenStart;
    private float tweenEnd;
    private Task task;

    public patch_WorkshopListLoader(MapScene map) : base(map)
    {
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void .ctor(System.Int32)")]
    [MonoModIgnore]
    public void base_ctor(int layerIndex = 0) { }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(MapScene map)
    {
        base_ctor(0);
        this.map = map;

        Depth = -100000;
        Visible = false;

        buttons = [new DiscoveryMapButton(), new WorkshopRandomSelect()];

        spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.BackOut, 18, false);
        spinTween.OnUpdate = t =>
        {
            spin = MathHelper.Lerp(tweenStart, tweenEnd, t.Eased);
        };
        Add(spinTween);
        Turn();

        Alarm.Set(this, 20, Turn, Alarm.AlarmMode.Looping);
        Vector2 start = Position = new Vector2(15f, 280f);
        Vector2 end = new Vector2(25f, 215f);

        introTween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, 4, true);
        introTween.OnUpdate = t =>
        {
            Position = Vector2.Lerp(start, end, t.Eased);
        };

        Add(introTween);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            task = new Task(Load);
            task.Start();
        }
        else
        {
            Load();
        }
    }

    [MonoModIgnore]
    private extern void Load();

    [MonoModIgnore]
    private extern void Turn();
}