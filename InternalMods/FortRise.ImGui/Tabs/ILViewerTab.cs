using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ImGuiNET;

namespace FortRise.ImGuiLib;

internal sealed class ILViewerTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "IL Viewer";
    private string searchBar = "";
    private MethodBase[]? methods;
    private string[] names = [];
    private MethodBase? selected;
    private string selectedName = "";
    private bool showTranspiled = true;
    private string[] lines = [];
    private string[] patchLines = [];

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        if (methods is null || ImGui.Button("Refresh"))
        {
            Refresh();
        }
        ImGui.SameLine();
        ImGui.InputText("Search", ref searchBar, 100);

        ImGui.BeginChild("Methods", new System.Numerics.Vector2(300, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
        for (int i = 0; i < methods!.Length; i++)
        {
            var name = names[i];
            if (!string.IsNullOrEmpty(searchBar) && !name.Contains(searchBar, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            ImGui.PushID(i);
            if (ImGui.Selectable(name, methods[i] == selected))
            {
                selectedName = name;
                Select(methods[i]);
            }
            ImGui.PopID();
        }
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("IL", System.Numerics.Vector2.Zero, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
        if (selected is not null)
        {
            ImGui.Text(selectedName);
            if (ImGui.Checkbox("Apply Transpilers", ref showTranspiled))
            {
                Select(selected);
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy"))
            {
                ImGui.SetClipboardText(string.Join('\n', lines));
            }

            ImGui.SeparatorText("Patches");
            foreach (var line in patchLines)
            {
                ImGui.TextUnformatted(line);
            }

            ImGui.SeparatorText("IL");
            foreach (var line in lines)
            {
                ImGui.TextUnformatted(line);
            }
        }
        ImGui.EndChild();
    }

    private void Refresh()
    {
        var patched = Harmony.GetAllPatchedMethods().ToArray();
        var duplicates = patched.GroupBy(GetName).Where(x => x.Count() > 1).Select(x => x.Key).ToHashSet();

        var entries = patched
            .Select(x => (Method: x, Name: duplicates.Contains(GetName(x)) ? GetName(x) + GetParameters(x) : GetName(x)))
            .OrderBy(x => x.Name)
            .ToArray();

        methods = [.. entries.Select(x => x.Method)];
        names = [.. entries.Select(x => x.Name)];
    }

    private static string GetParameters(MethodBase method)
    {
        return "(" + string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}")) + ")";
    }

    private void Select(MethodBase method)
    {
        selected = method;
        try
        {
            var instructions = showTranspiled
                ? PatchProcessor.GetCurrentInstructions(method)
                : PatchProcessor.GetOriginalInstructions(method);
            lines = [.. instructions.Select((x, i) => $"{i:D4}  {x}")];
        }
        catch (Exception e)
        {
            lines = ["Failed to read IL: " + e.Message];
        }

        var info = Harmony.GetPatchInfo(method);
        if (info is null)
        {
            patchLines = [];
            return;
        }

        var list = new List<string>();
        AddPatches(list, "Prefix", info.Prefixes);
        AddPatches(list, "Postfix", info.Postfixes);
        AddPatches(list, "Transpiler", info.Transpilers);
        AddPatches(list, "Finalizer", info.Finalizers);
        patchLines = [.. list];
    }

    private static void AddPatches(List<string> list, string kind, IEnumerable<Patch> patches)
    {
        foreach (var patch in patches)
        {
            list.Add($"[{kind}] {GetName(patch.PatchMethod)} ({patch.owner}, priority {patch.priority})");
        }
    }

    private static string GetName(MethodBase method)
    {
        return $"{method.DeclaringType?.FullName}::{method.Name}";
    }
}
