using System;
using ImGuiNET;
using Monocle;

namespace FortRise.ImGuiLib;

internal sealed class ConsoleTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Console";
    private string input = "";
    private bool refocus;

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        var commands = Engine.Instance.Commands;

        float footer = ImGui.GetFrameHeightWithSpacing();
        if (ImGui.BeginChild("Output", new System.Numerics.Vector2(0, -footer), ImGuiChildFlags.Borders))
        {
            for (int i = commands.drawCommands.Count - 1; i >= 0; i--)
            {
                ImGui.TextUnformatted(commands.drawCommands[i]);
            }
            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1f);
            }
        }
        ImGui.EndChild();

        if (refocus)
        {
            ImGui.SetKeyboardFocusHere();
            refocus = false;
        }

        ImGui.SetNextItemWidth(-ImGui.CalcTextSize("Clear").X - ImGui.GetStyle().FramePadding.X * 2 - ImGui.GetStyle().ItemSpacing.X);
        if (ImGui.InputText("##Command", ref input, 256, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Execute(commands);
            refocus = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear"))
        {
            commands.Clear();
        }
    }

    private void Execute(Commands commands)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            commands.currentText = input;
            try
            {
                commands.EnterCommand();
            }
            catch (Exception e)
            {
                commands.Log(e.Message);
            }
        }
        input = "";
    }
}
