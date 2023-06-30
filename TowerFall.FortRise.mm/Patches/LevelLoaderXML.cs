using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_LevelLoaderXML : LevelLoaderXML
{
    private Coroutine loader;
    private bool errorShown;
    public patch_LevelLoaderXML(Session session) : base(session)
    {
    }

    public extern void orig_ctor(Session session);

    [MonoModConstructor]
    public void ctor(Session session) 
    {
        RiseCore.Events.Invoke_OnLevelLoaded();
        orig_ctor(session);
        if (XML == null) 
        {
            SetLayer(-1, new Layer());
            loader = null;
            Sounds.ui_click.Play(160f, 1f);
            session.MatchSettings.LevelSystem.Dispose();
        }
    }

    [MonoModLinkTo("Monocle.Scene", "System.Void Update()")]
    [MonoModIgnore]
    public void base_Update() { base.Update(); }

    [MonoModLinkTo("Monocle.Scene", "System.Void Render()")]
    [MonoModIgnore]
    public void base_Render() { base.Render(); }


    [MonoModReplace]
    public override void Update()
    {
        if (loader == null && !errorShown) 
        {
            errorShown = true;
            patch_SaveData.AdventureActive = false;
            this.ShowError("Missing Level");
        }
        if (errorShown) 
        {
            MenuInput.Update();
            base_Update();
            return;
        }
        loader.Update();
    }

    public extern void orig_Render();

    public override void Render()
    {
        if (errorShown)
        {
            base_Render();
            return;
        }
        orig_Render();
    }
}