using System;
using System.Reflection;
using HarmonyLib;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace FortRise.ImGuiLib;

internal sealed class ImGuiModule : FortModule
{
    private bool imguiOpened;
    private bool mouseOpened;
    private ImGuiRenderer renderer = null!;


    private Hook hook_Engine_Draw = null!;
    private Hook hook_Commands_UpdateOpen = null!;
    private Hook hook_Commands_Render = null!;

    public override Type SettingsType => typeof(ImGuiSettings);
    public ImGuiSettings Settings => (ImGuiSettings)InternalSettings;
    public static ImGuiModule Instance { get; private set; } = null!;

    public override void Load()
    {
        Instance = this;
        Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Engine), "Draw"),
            new HarmonyMethod(Engine_Draw_Postfix)
        );

        Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Commands), "HandleKey"),
            new HarmonyMethod(Commands_HandleKey_Prefix)
        );

        Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Commands), "Render"),
            new HarmonyMethod(Commands_Render_Prefix)
        );
    }

    public override void Unload()
    {
    }

    public override object? GetApi() => new ApiImplementation();
    

    public override void Initialize()
    {
        renderer = new ImGuiRenderer(Engine.Instance);
        renderer.RebuildFontAtlas();

        TabItemManager.Instance.Register(new SceneInfoTab());
        TabItemManager.Instance.Register(new ArrowTab());
        TabItemManager.Instance.Register(new PickupTab());
        TabItemManager.Instance.Register(new EnemyTab());
    }

    private static bool Commands_HandleKey_Prefix(Commands __instance, Keys key)
    {
        if (!ImGuiModule.Instance.Settings.IsEnabled)
        {
            return true;
        }

        if (key == Keys.OemTilde)
        {
            DynamicData.For(__instance).Set("canOpen", false);
            __instance.Open = false;
        }

        return false;
    }

    private static bool Commands_Render_Prefix(Action<Commands> orig, Commands self)
    {
        return !ImGuiModule.Instance.Settings.IsEnabled;
    }

    private static void Engine_Draw_Postfix(Engine __instance, GameTime gameTime)
    {
        if (!ImGuiModule.Instance.Settings.IsEnabled)
        {
            return;
        }
        var renderer = ImGuiModule.Instance.renderer;
        if (renderer != null && __instance.Commands.Open)
        {
            if (!ImGuiModule.Instance.imguiOpened)
            {
                ImGuiModule.Instance.mouseOpened = Engine.Instance.IsMouseVisible;
            }
            renderer.BeforeLayout(gameTime);
            ImGui.Begin("Debug Window");

            if (ImGui.BeginTabBar("Features_Tab"))
            {

                foreach (var tab in TabItemManager.Instance.Tabs)
                {
                    if (ImGui.BeginTabItem(tab.Title))
                    {
                        tab.Render(renderer);
                        ImGui.EndTabItem();
                    }
                }


                ImGui.EndTabBar();
            }


            ImGui.End();
            renderer.AfterLayout();
            ImGuiModule.Instance.imguiOpened = true;
        }
        else
        {
            ImGuiModule.Instance.imguiOpened = false;
            Engine.Instance.IsMouseVisible = ImGuiModule.Instance.imguiOpened;
        }

        if (ImGuiModule.Instance.imguiOpened)
        {
            Engine.Instance.IsMouseVisible = true;
        }
    }
}
