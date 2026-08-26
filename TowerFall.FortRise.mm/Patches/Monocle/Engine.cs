using System;
using System.Runtime.CompilerServices;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using TowerFall;

namespace Monocle 
{
    public class patch_Engine : Engine
    {
        private Scene scene;
        private Scene nextScene;


        public patch_Commands Commands { [MonoModIgnore] get => null; [MonoModIgnore] private set => throw new System.Exception(value.ToString()); }

        public static float TimeMult
        {
            [MonoModIgnore]
            get
            {
                return 0;
            }
            [MonoModIgnore]
            private set {}
        }

        public static float LastTimeMult
        {
            [MonoModIgnore]
            get
            {
                return 0;
            }
            [MonoModIgnore]
            private set {}
        }
        public static float DeltaTime
        {
            [MonoModIgnore]
            get
            {
                return 0;
            }
            [MonoModIgnore]
            private set {}
        }

        public static float ActualDeltaTime
        {
            [MonoModIgnore]
            get
            {
                return 0;
            }
            [MonoModIgnore]
            private set
            {
            }
        }

        public static long DeltaTicks
        {
            [MonoModIgnore]
            get
            {
                return 0;
            }
            [MonoModIgnore]
            private set
            {
            }
        }

        public Scene NextScene
        {
            [MonoModIgnore]
            get
            {
                return null;
            }
            [MonoModIgnore]
            private set {}
        }

        public Scene PreviousScene
        {
            [MonoModIgnore]
            get
            {
                return null;
            }
            [MonoModIgnore]
            private set {}
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<IsFixedTimeStep>k__BackingField")]
        private static extern ref bool backingField_IsFixedTimeStep(Game game);
        
        public patch_Engine(int width, int height, float scale, string windowTitle) : base(width, height, scale, windowTitle)
        {
        }

        [MonoModLinkTo("Microsoft.Xna.Framework.Game", "System.Void Initialize()")]
        [MonoModIgnore]
        protected void base_Initialize() 
        {
            base.Initialize();
        }

        [MonoModLinkTo("Microsoft.Xna.Framework.Game", "System.Void Update(Microsoft.Xna.Framework.GameTime)")]
        [MonoModIgnore]
        protected void base_Update(GameTime gameTime) 
        {
            base.Update(gameTime);
        }

        [MonoModLinkFrom("System.Void Microsoft.Xna.Framework.Game::set_IsFixedTimeStep(System.Boolean)")]
        public void EnableFixedTimeStep(bool value)
        {
            if (value)
            {
                backingField_IsFixedTimeStep(this) = true;
                Instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 240f);
            }
            else 
            {
                backingField_IsFixedTimeStep(this) = false;
                // DO NOT TOUCH THIS
                Instance.TargetElapsedTime = TimeSpan.FromSeconds(0.016666666666666666);
            }
        }

        [MonoModReplace]
        protected override void Initialize() 
        {
            base_Initialize();
            Graphics.DeviceReset += OnGraphicsReset;
            Graphics.DeviceCreated += OnGraphicsCreated;
            patch_MInput.Initialize();
            Commands = new patch_Commands();

            EnableFixedTimeStep(FortRiseModule.Settings.FixedTimeStep);
        }

        [MonoModReplace]
        protected override void Update(GameTime gameTime)
        {
            LastTimeMult = TimeMult;
            ActualDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * TimeRate;
            DeltaTicks = gameTime.ElapsedGameTime.Ticks;
            if (IsFixedTimeStep)
            {
                TimeMult = ActualDeltaTime * 60;
                DeltaTime = Math.Min(ActualDeltaTime, (float)TargetElapsedTime.TotalSeconds * (TimeRate + (float)TargetElapsedTime.TotalSeconds / 2f));
            }
            else
            {
                DeltaTime = Math.Min(Engine.ActualDeltaTime, 0.016666668f * (TimeRate + 0.008333334f));
                TimeMult = Engine.DeltaTime / 0.016666668f;
            }

            patch_MInput.Update();

            if (scene != null && scene.Active)
            {
                scene.Update();
            }

            if (ConsoleEnabled)
            {
                if (Commands.Open)
                {
                    Commands.UpdateOpen();
                }
                else
                {
                    Commands.UpdateClosed();
                }
            }
            if (scene != nextScene)
            {
                NextScene = nextScene;
                PreviousScene = scene;
                scene?.End();
                scene = nextScene;

                OnSceneTransition();

                scene?.Begin();
                NextScene = PreviousScene = null;
            }
            base_Update(gameTime);
            NotificationManager.Instance.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        protected override void Dispose(bool disposing)
        {
            // make sure to unbind all rendertarget first
            // and for the love of god, please stop putting exception on Dispose method
            GraphicsDevice.SetRenderTarget(null);

            base.Dispose(disposing);
        }

        [MonoModIgnore]
        [PatchEngineDraw]
        protected extern override void Draw(GameTime gameTime);

        public static void RenderNotifications()
        {
            Monocle.Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                DepthStencilState.None, 
                RasterizerState.CullNone
            );

            var font = TFGame.Font;
            float screenWidth = Engine.Instance.Screen.Width;

            foreach (var message in NotificationManager.Instance.Messages)
            {
                if (message.Lines is null)
                {
                    continue;
                }

                float restX = screenWidth - NotificationManager.Margin - message.Width;
                float x = MathHelper.Lerp(screenWidth, restX, message.Slide);
                float y = message.Y;

                Color accent = message.Type switch
                {
                    NotificationType.Warning => Color.Gold,
                    NotificationType.Error => Color.Red,
                    _ => Color.White
                };

                Monocle.Draw.Rect(x - 1, y - 1, message.Width + 2, message.Height + 2, accent);
                Monocle.Draw.Rect(x, y, message.Width, message.Height, Color.Black);
                Monocle.Draw.Rect(x, y, NotificationManager.AccentWidth, message.Height, accent);

                float textX = x + NotificationManager.AccentWidth + NotificationManager.Padding;
                float textY = y + NotificationManager.Padding;

                Monocle.Draw.OutlineTextJustify(font, message.Header, new Vector2(textX, textY), Color.Gray, Color.Black, Vector2.Zero);

                for (int i = 0; i < message.Lines.Length; i += 1)
                {
                    textY += NotificationManager.LineHeight;
                    Monocle.Draw.OutlineTextJustify(font, message.Lines[i], new Vector2(textX, textY), Color.White, Color.Black, Vector2.Zero);
                }
            }

            Monocle.Draw.SpriteBatch.End();

        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchEngineDraw))]
    internal class PatchEngineDraw : Attribute {}

    internal static partial class MonoModRules
    {
        public static void PatchEngineDraw(ILContext ctx, CustomAttribute attrib) 
        {
            var RenderNotification = ctx.Method.DeclaringType.FindMethod("System.Void RenderNotifications()");
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("Monocle.Scene", "PostRender"));
            cursor.Emit(OpCodes.Call, RenderNotification);
        }
    }
}

