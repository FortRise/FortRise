using System;
using System.Collections.Generic;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_Level : Level
    {
        private Layer bgGameLayer;
        private Layer gameLayer;
        private RenderTarget2D foregroundRenderTarget;
        private List<SceneFilter> ActiveShaders;
        private List<Entity> xmlEntities;

        public XmlElement XML
        {
            [MonoModIgnore]
            get => throw null;
            [MonoModIgnore]
            private set => throw null;
        }

        public static bool DebugMode;
        public patch_Level(Session session, XmlElement xml) : base(session, xml)
        {
        }


        [Postfix("System.Void .ctor(TowerFall.Session,System.Xml.XmlElement)")]
        private void InitializeModdedLevel(Session session)
        {
            ActiveShaders = new();
            xmlEntities = new();
        }

        public extern void orig_LoadEntity(XmlElement e);

        public void LoadEntity(XmlElement e)
        {
            var previous = patch_Layer.Recorder;
            patch_Layer.Recorder = xmlEntities;
            try
            {
                if (EntityRegistry.EntityEntries.TryGetValue(e.Name, out var entity))
                {
                    if (entity.Configuration.IsHazard && Session.MatchSettings.Variants.NoHazards)
                    {
                        return;
                    }

                    Add(entity.Configuration.Loader(e, e.Position(), e.Nodes()));
                    return;
                }

                orig_LoadEntity(e);
            }
            finally
            {
                patch_Layer.Recorder = previous;
            }
        }

        internal List<Entity> RebuildLayout(XmlElement xml)
        {
            XML = xml;

            foreach (var entity in xmlEntities)
            {
                if (entity.Scene == this)
                {
                    Remove(entity);
                }
            }
            xmlEntities.Clear();
            Remove(Tiles);
            Remove(BGTiles);

            bool[,] solidsBitData = Calc.GetBitData(xml["Solids"].InnerText, 32, 24);
            bool[,] bgBitData = Calc.GetBitData(xml["BG"].InnerText, 32, 24);
            int[,] overwriteData = Calc.ReadCSVIntGrid(xml["BGTiles"].InnerText, 32, 24);

            Add(Tiles = new LevelTiles(xml, solidsBitData));
            Add(BGTiles = new LevelBGTiles(xml, bgBitData, solidsBitData, overwriteData));

            foreach (XmlElement e in xml["Entities"])
            {
                LoadEntity(e);
            }

            return [Tiles, BGTiles, .. xmlEntities];
        }

        internal void RefreshPlayers()
        {
            foreach (var entity in this[GameTags.Player])
            {
                if (entity is not patch_Player player || player.Dead || player.MarkedForRemoval)
                {
                    continue;
                }

                if (player.ArcherData == ArcherData.Get(player.CharacterIndex, player.AltSelect))
                {
                    continue;
                }

                Remove(player);
                Add(player.CloneForHotReload());
            }
        }

        [MonoModReplace]
        public void CoreRender(RenderTarget2D canvas) 
        {
            Engine.Instance.GraphicsDevice.SetRenderTarget(foregroundRenderTarget);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            bgGameLayer.Render();
            gameLayer.Render();
            if (Foreground != null && Foreground.Visible)
            {
                Foreground.Render();
            }

            Engine.Instance.GraphicsDevice.SetRenderTarget(canvas);
            if (Background != null && Background.Visible)
            {
                Background.Render();
            }
            if (LightingLayer.DarkenForLightning)
            {
                TFGame.LightingAlpha.SetValue(Math.Min(1f, LightingLayer.Alpha * 1.5f));
            }
            else
            {
                TFGame.LightingAlpha.SetValue(LightingLayer.Alpha);
            }
            foreach (var shaders in ActiveShaders) 
            {
                shaders.BeforeRender(canvas);
            }

            Engine.Instance.GraphicsDevice.Textures[1] = LightingLayer.Canvas.Texture2D;
            Draw.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, TFGame.LightingEffect, Matrix.Identity);
            Draw.SpriteBatch.Draw(foregroundRenderTarget, Vector2.Zero, Color.White);
            foreach (var shader in ActiveShaders) 
            {
                shader.Render(canvas);
            }
            Draw.SpriteBatch.End();
            foreach (var shader in ActiveShaders) 
            {
                shader.AfterRender(canvas);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            if (Camera.Origin == Vector2.Zero)
            {
                if (Camera.X < 0f)
                {
                    Draw.Rect(0f, -2f, 2f, 244f, Color.Black);
                }
                else if (Camera.X > 0f)
                {
                    Draw.Rect(318f, -2f, 2f, 244f, Color.Black);
                }
                if (Camera.Y < 0f)
                {
                    Draw.Rect(-2f, 0f, 324f, 2f, Color.Black);
                }
                else if (Camera.Y > 0f)
                {
                    Draw.Rect(-2f, 238f, 324f, 2f, Color.Black);
                }
            }
            Draw.SpriteBatch.End();
        }

        [Prefix(nameof(Begin))]
        public void PreBegin()
        {
            ModEventsManager.Instance.LevelEntered.Raise(this, this);
        }

        [Postfix(nameof(End))]
        private void PostEnd() 
        {
            ModEventsManager.Instance.LevelExited.Raise(this, this);
        }

        [Postfix(nameof(Render))]
        public void DebugModeRender() 
        {
            if (DebugMode) 
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Lerp(Matrix.Identity, Camera.Matrix, 1f));
                foreach (var entity in Layers[0].Entities) 
                {
                    entity.DebugRender();
                }
                Draw.SpriteBatch.End();
            }
        }

        [PatchLevelHandlePausing]
        [MonoModIgnore]
        public extern void HandlePausing();

        public void Activate(SceneFilter filter) 
        {
            ActiveShaders.Add(filter);
            filter.InternalActivated(new SceneFilter.LevelRenderData { 
                ForegroundRenderTarget = foregroundRenderTarget,
                BGTiles = BGTiles,
                SolidTiles = Tiles,
                Level = this
            });
        }

        public void Deactivate(SceneFilter filter) 
        {
            ActiveShaders.Remove(filter);
            filter.Deactivated();
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchLevelHandlePausing))]
    internal class PatchLevelHandlePausing : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchLevelHandlePausing(ILContext ctx, CustomAttribute attrib) 
        {
            var get_NoAutoPause = ctx.Module.GetType("FortRise.RiseCore").FindMethod("get_NoAutoPause");
            var cursor = new ILCursor(ctx);
            ILLabel label = null;

            cursor.GotoNext(instr => instr.MatchCallOrCallvirt(
                "Microsoft.Xna.Framework.Game", "get_IsActive"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchBrtrue(out label));

            cursor.Emit(OpCodes.Call, get_NoAutoPause);
            cursor.Emit(OpCodes.Brtrue_S, label);
        }
    }
}
