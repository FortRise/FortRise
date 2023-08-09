using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_PauseMenu : PauseMenu
{
    private PauseMenu.MenuType menuType;
    private Level level;


    public patch_PauseMenu(Level level, Vector2 position, MenuType menuType, int controllerDisconnected = -1) : base(level, position, menuType, controllerDisconnected)
    {
    }

    [MonoModReplace]
    private void DarkWorldRestart() 
    {
        Sounds.ui_click.Play(160f, 1f);
        var oldLevelSet = this.level.Session.GetLevelSet();
        var session = new Session(this.level.Session.MatchSettings);
        session.SetLevelSet(oldLevelSet); 
        session.StartGame();
    }

    [MonoModReplace]
    private void DarkWorldMap() 
    {
        patch_DarkWorldControl.DisableTempVariants(level);
        Sounds.ui_click.Play(160f, 1f);
        var mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
        Engine.Instance.Scene = mapScene;
        mapScene.SetLevelSet(level.Session.GetLevelSet());
        this.level.Session.MatchSettings.LevelSystem.Dispose();
    }

    [MonoModReplace]
    private void DarkWorldMapAndSave() 
    {
        patch_DarkWorldControl.DisableTempVariants(level);
        Sounds.ui_click.Play(160f, 1f);
        MapScene mapScene = new MapScene(MainMenu.RollcallModes.DarkWorld);
        mapScene.ShouldSave = true;
        Engine.Instance.Scene = mapScene;
        mapScene.SetLevelSet(level.Session.GetLevelSet());
        this.level.Session.MatchSettings.LevelSystem.Dispose();
    }

    [MonoModReplace]
    private void QuestMap()
    {
        Sounds.ui_click.Play(160f, 1f);
        MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
        Engine.Instance.Scene = mapScene;
        mapScene.SetLevelSet(level.Session.GetLevelSet());
        this.level.Session.MatchSettings.LevelSystem.Dispose();
    }

    
    [MonoModReplace]
    private void QuestMapAndSave()
    {
        Sounds.ui_click.Play(160f, 1f);
        MapScene mapScene = new MapScene(MainMenu.RollcallModes.Quest);
        mapScene.ShouldSave = true;
        Engine.Instance.Scene = mapScene;
        mapScene.SetLevelSet(level.Session.GetLevelSet());
        this.level.Session.MatchSettings.LevelSystem.Dispose();
    }

    private extern void orig_Quit();

    private void Quit() 
    {
        patch_DarkWorldControl.DisableTempVariants(level);
        orig_Quit();
    }

    private extern void orig_QuitAndSave();

    public void QuitAndSave() 
    {
        patch_DarkWorldControl.DisableTempVariants(level);
        orig_QuitAndSave();
    }

    [MonoModReplace]
    private void VersusRematch() 
    {
        Sounds.ui_click.Play(160f, 1f);
        MapScene mapScene = new MapScene(MainMenu.RollcallModes.Versus);
        Engine.Instance.Scene = mapScene;
        mapScene.SetLevelSet(level.Session.GetLevelSet());
        this.level.Session.MatchSettings.LevelSystem.Dispose();
    }
}