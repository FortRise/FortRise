using System;
using ImGuiNET;
using Monocle;

namespace FortRise.ImGuiLib;

internal sealed class PerformanceTab : IFortRiseImGuiAPI.ITabItem
{
    public string Title => "Performance";
    private readonly float[] frameTimes = new float[120];
    private int frameOffset;

    public void Render(IFortRiseImGuiAPI.IRenderer renderer)
    {
        var io = ImGui.GetIO();
        frameTimes[frameOffset] = io.DeltaTime * 1000f;
        frameOffset = (frameOffset + 1) % frameTimes.Length;

        ImGui.SeparatorText("Frame");
        ImGui.Text($"FPS: {io.Framerate:0.0}");
        ImGui.PlotLines("Frame Time (ms)", ref frameTimes[0], frameTimes.Length, frameOffset, "", 0f, 50f, new System.Numerics.Vector2(0, 60));

        ImGui.SeparatorText("Memory");
        ImGui.Text($"Managed Heap: {GC.GetTotalMemory(false) / (1024f * 1024f):0.00} MB");
        ImGui.Text($"GC Collections: {GC.CollectionCount(0)} / {GC.CollectionCount(1)} / {GC.CollectionCount(2)}");

        ImGui.SeparatorText("Time");
        ImGui.SliderFloat("Time Rate", ref Engine.TimeRate, 0f, 2f);
        if (ImGui.Button("Reset"))
        {
            Engine.TimeRate = 1f;
        }
    }
}
