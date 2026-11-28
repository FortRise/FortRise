using System;
using System.Collections.Generic;
using HarmonyLib;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;

namespace FortRise.ImGuiLib;

internal sealed class ImGuiModule : Mod
{
    private bool imguiOpened;
    private bool mouseOpened;
    private ImGuiRenderer? renderer = null!;
    private readonly HashSet<IFortRiseImGuiAPI.ITabItem> closedTabs = [];
    private RenderTarget2D? gameTexture;
    private IntPtr gameTextureID;
    private bool gameFocused;

    private static readonly BlendState OpaqueColorOnly = new()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.Zero,
        ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue
    };

    public ImGuiModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
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

        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(MInput), "Update"),
            prefix: new HarmonyMethod(MInput_Update_Prefix),
            postfix: new HarmonyMethod(MInput_Update_Postfix)
        );

        TabItemManager.Instance.Register(new DebugTab());
        TabItemManager.Instance.Register(new ArrowTab());
        TabItemManager.Instance.Register(new PickupTab());
        TabItemManager.Instance.Register(new EnemyTab());
        TabItemManager.Instance.Register(new EntityTab());
        TabItemManager.Instance.Register(new PerformanceTab());
        TabItemManager.Instance.Register(new ILViewerTab());
        TabItemManager.Instance.Register(new ConsoleTab());
        TabItemManager.Instance.Register(new SubtextureTab());
        TabItemManager.Instance.Register(new SpriteTab());

        OnInitialize += OnInitialization;
    }

    private void OnInitialization(IModuleContext context)
    {
        try
        {
            // renderer must be initialized after the game launched
            renderer = new ImGuiRenderer(Engine.Instance);
            renderer.RebuildFontAtlas();
        }
        catch (DllNotFoundException)
        {
            renderer = null;
        }
    }

    public ImGuiSettings Settings => Instance.GetSettings<ImGuiSettings>()!;
    public static ImGuiModule Instance { get; private set; } = null!;

    public override ModuleSettings? CreateSettings()
    {
        return new ImGuiSettings();
    }


    public override object? GetApi(ModuleMetadata metadata) => new ApiImplementation();


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

    private static void MInput_Update_Prefix(out bool __state)
    {
        var commands = Engine.Instance.Commands;
        __state = ImGuiModule.Instance.Settings.IsEnabled
            && ImGuiModule.Instance.renderer != null
            && commands.Open
            && (ImGuiModule.Instance.gameFocused || !ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow));

        if (__state)
        {
            commands.Open = false;
        }
    }

    private static void MInput_Update_Postfix(bool __state)
    {
        if (__state)
        {
            Engine.Instance.Commands.Open = true;
        }
    }

    private void RenderGameWindow(ImGuiRenderer renderer, uint dockID)
    {
        var device = Engine.Instance.GraphicsDevice;
        var source = Engine.Instance.Screen.RenderTarget;
        if (gameTexture == null || gameTexture.Width != source.Width || gameTexture.Height != source.Height)
        {
            if (gameTexture != null)
            {
                renderer.UnbindTexture(gameTextureID);
                gameTexture.Dispose();
            }
            gameTexture = new RenderTarget2D(device, source.Width, source.Height);
            gameTextureID = renderer.BindTexture(gameTexture);
        }

        // the game's render target has unreliable alpha, so copy it with alpha forced to opaque
        device.SetRenderTarget(gameTexture);
        device.Clear(Color.Black);
        Monocle.Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, OpaqueColorOnly, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        Monocle.Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
        Monocle.Draw.SpriteBatch.End();
        device.SetRenderTarget(null);

        var target = gameTexture;

        ImGui.SetNextWindowDockID(dockID, ImGuiCond.FirstUseEver);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
        if (ImGui.Begin("Game"))
        {
            var avail = ImGui.GetContentRegionAvail();
            float scale = MathF.Min(avail.X / target.Width, avail.Y / target.Height);
            if (scale > 0)
            {
                var size = new System.Numerics.Vector2(target.Width * scale, target.Height * scale);
                ImGui.SetCursorPos(ImGui.GetCursorPos() + (avail - size) / 2);
                ImGui.Image(gameTextureID, size);
            }
        }
        gameFocused = ImGui.IsWindowFocused();
        ImGui.End();
        ImGui.PopStyleVar();
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
            var settings = ImGuiModule.Instance.Settings;
            bool editorMode = settings.EditorMode;
            uint viewportDockID = ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), editorMode ? ImGuiDockNodeFlags.None : ImGuiDockNodeFlags.PassthruCentralNode);

            if (editorMode)
            {
                ImGuiModule.Instance.RenderGameWindow(renderer, viewportDockID);
            }
            else
            {
                ImGuiModule.Instance.gameFocused = false;
            }

            var closedTabs = ImGuiModule.Instance.closedTabs;
            uint dockID = ImGui.GetID("DebugDockSpace");
            if (ImGui.Begin("Debug Window", ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("View"))
                    {
                        if (ImGui.MenuItem("Editor Mode", "", editorMode))
                        {
                            settings.EditorMode = !editorMode;
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Windows"))
                    {
                        foreach (var tab in TabItemManager.Instance.Tabs)
                        {
                            if (ImGui.MenuItem(tab.Title, "", !closedTabs.Contains(tab)) && !closedTabs.Remove(tab))
                            {
                                closedTabs.Add(tab);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }
                ImGui.DockSpace(dockID);
            }
            else
            {
                ImGui.DockSpace(dockID, System.Numerics.Vector2.Zero, ImGuiDockNodeFlags.KeepAliveOnly);
            }
            ImGui.End();

            foreach (var tab in TabItemManager.Instance.Tabs)
            {
                if (closedTabs.Contains(tab))
                {
                    continue;
                }

                bool open = true;
                ImGui.SetNextWindowDockID(dockID, ImGuiCond.FirstUseEver);
                if (ImGui.Begin(tab.Title, ref open))
                {
                    tab.Render(renderer);
                }
                ImGui.End();

                if (!open)
                {
                    closedTabs.Add(tab);
                }
            }

            if (editorMode)
            {
                __instance.GraphicsDevice.Clear(Color.Black);
            }

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
