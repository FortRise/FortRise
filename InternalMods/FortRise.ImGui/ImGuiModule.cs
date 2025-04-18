using System;
using System.Reflection;
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

    public override void Load()
    {
        hook_Engine_Draw = new Hook(
            typeof(Engine).GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance)!,
            Engine_Draw_patch
        );

        hook_Commands_UpdateOpen = new Hook(
            typeof(Commands).GetMethod("HandleKey", BindingFlags.NonPublic | BindingFlags.Instance)!,
            Commands_HandleKey_patch
        );

        hook_Commands_Render = new Hook(
            typeof(Commands).GetMethod("Render", BindingFlags.NonPublic | BindingFlags.Instance)!,
            Commands_Render_patch
        );
    }

    public override void Unload()
    {
        hook_Engine_Draw.Dispose();
        hook_Commands_UpdateOpen.Dispose();
        hook_Commands_Render.Dispose();
    }

    public override object? GetApi() => new ApiImplementation();
    

    public override void Initialize()
    {
        renderer = new ImGuiRenderer(Engine.Instance);
        renderer.RebuildFontAtlas();

        TabItemManager.Instance.Register(new ArrowTab());
        TabItemManager.Instance.Register(new PickupTab());
        TabItemManager.Instance.Register(new EnemyTab());
    }

    private void Commands_HandleKey_patch(Action<Commands, Keys> orig, Commands self, Keys key)
    {
        if (!Settings.IsEnabled)
        {
            orig(self, key);
            return;
        }
        if (key == Keys.OemTilde)
        {
            DynamicData.For(self).Set("canOpen", false);
            self.Open = false;
        }
    }

    private void Commands_Render_patch(Action<Commands> orig, Commands self)
    {
        if (Settings.IsEnabled)
        {
            return;
        }
        orig(self);
    }

    private void Engine_Draw_patch(Action<Engine, GameTime> orig, Engine self, GameTime gameTime)
    {
        orig(self, gameTime);
        if (!Settings.IsEnabled)
        {
            return;
        }
        if (renderer != null && self.Commands.Open)
        {
            if (!imguiOpened)
            {
                mouseOpened = Engine.Instance.IsMouseVisible;
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
            imguiOpened = true;
        }
        else 
        {
            imguiOpened = false;
            Engine.Instance.IsMouseVisible = mouseOpened;
        }

        if (imguiOpened)
        {
            Engine.Instance.IsMouseVisible = true;
        }
    }
}
