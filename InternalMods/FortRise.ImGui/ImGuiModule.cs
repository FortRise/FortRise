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

internal sealed class ImGuiModule : Mod
{
    private bool imguiOpened;
    private bool mouseOpened;
    private ImGuiRenderer renderer = null!;

    public ImGuiModule(IModContent content, IModuleContext context) : base(content, context)
    {
        Instance = this;

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Engine), "Draw"),
            postfix: new HarmonyMethod(Engine_Draw_Postfix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Commands), "HandleKey"),
            prefix: new HarmonyMethod(Commands_HandleKey_Prefix)
        );

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Commands), "Render"),
            prefix: new HarmonyMethod(Commands_Render_Prefix)
        );

        TabItemManager.Instance.Register(new SceneInfoTab());
        TabItemManager.Instance.Register(new ArrowTab());
        TabItemManager.Instance.Register(new PickupTab());
        TabItemManager.Instance.Register(new EnemyTab());

        OnInitialize += OnInitialization;
    }

    private void OnInitialization(IModuleContext context)
    {
        renderer = new ImGuiRenderer(Engine.Instance);
        renderer.RebuildFontAtlas();
    }

    public ImGuiSettings Settings => Instance.GetSettings<ImGuiSettings>()!;
    public static ImGuiModule Instance { get; private set; } = null!;

    public override ModuleSettings? CreateSettings()
    {
        return new ImGuiSettings();
    }


    public override object? GetApi() => new ApiImplementation();
    

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

    private static bool Commands_Render_Prefix()
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
                ImGuiModule.Instance.mouseOpened = __instance.IsMouseVisible;
            }

            if (gameTime.ElapsedGameTime == TimeSpan.Zero) // prevents crash when moving a window
            {
                goto SKIP;
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

            SKIP:
            renderer.AfterLayout();
            ImGuiModule.Instance.imguiOpened = true;
        }
        else
        {
            ImGuiModule.Instance.imguiOpened = false;
            __instance.IsMouseVisible = ImGuiModule.Instance.imguiOpened;
        }

        if (ImGuiModule.Instance.imguiOpened)
        {
            __instance.IsMouseVisible = true;
        }
    }
}
