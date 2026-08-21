using System;
using System.Runtime.CompilerServices;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;

namespace Monocle;

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
    }

    protected override void Dispose(bool disposing)
    {
        // make sure to unbind all rendertarget first
        // and for the love of god, please stop putting exception on Dispose method
        GraphicsDevice.SetRenderTarget(null);

        base.Dispose(disposing);
    }
}
