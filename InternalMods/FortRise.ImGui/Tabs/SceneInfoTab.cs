using ImGuiNET;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class SceneInfoTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Scene Info";

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        var scene = Engine.Instance.Scene;
        if (scene is null)
        {
            return;
        }
        ImGui.Text("Scene: " + scene.GetType().FullName);
        ImGui.Text("Entities: " + scene.EntityCount);
        if (scene is Level level)
        {
            var levelSystem = level.Session.MatchSettings.LevelSystem;
            string levelID = levelSystem switch
            {
            DarkWorldLevelSystem dwSystem => dwSystem.DarkWorldTowerData.GetLevelID(),
            TrialsLevelSystem lSystem => lSystem.TrialsLevelData.GetLevelID(),
            QuestLevelSystem qSystem => qSystem.QuestTowerData.GetLevelID(),
            VersusLevelSystem vSystem => vSystem.VersusTowerData.GetLevelID(),
            _ => "Unidentified level system"
            };

            ImGui.Text("Level ID: " + levelID);
            ImGui.Text("Level Set: " + level.Session.GetLevelSet());
            ImGui.Text("Tags: " + string.Join(',', level.SceneTags));
        }
    }
}
