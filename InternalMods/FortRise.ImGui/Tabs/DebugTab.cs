using ImGuiNET;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise.ImGuiLib;

internal sealed class DebugTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Debug";

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        ImGui.SeparatorText("Entities");
        ImGui.Checkbox("Show Hitbox", ref Level.DebugMode);
        ImGui.SeparatorText("Scene");
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
            ImGui.Text("Level Set: " + level.Session.TowerSet);
            ImGui.Text("Tags: " + string.Join(',', level.SceneTags));
        }

        var numerics = new System.Numerics.Vector2(Engine.Instance.Screen.OffsetAdd.X, Engine.Instance.Screen.OffsetAdd.Y);

        ImGui.InputFloat2("OffsetAdd: ", ref numerics);
        Engine.Instance.Screen.OffsetAdd = new Vector2(numerics.X, numerics.Y);
    }
}
