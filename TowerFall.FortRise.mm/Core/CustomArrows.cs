using System;
using Monocle;
using FortRise;

namespace FortRise 
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomArrowsAttribute : Attribute 
    {
        public string Name;
        public string GraphicPickupInitializer;
        public float Chance = 1f;

        public CustomArrowsAttribute(string name, string graphicPickupFn = null) 
        {
            Name = name;
            GraphicPickupInitializer = graphicPickupFn;
        }

        public CustomArrowsAttribute(string name, float chance, string graphicPickupFn = null) 
        {
            Name = name;
            GraphicPickupInitializer = graphicPickupFn;
            Chance = chance;
        }
    }
}


namespace TowerFall 
{
    public class ArrowObject
    {
        public ArrowTypes Types;
        public PickupObject PickupType;
        public ArrowInfoLoader InfoLoader;
    }


    public struct ArrowInfo 
    {
        public string Color = "F7EAC3";
        public string ColorB = "FFFFFF";
        public string Name = "";
        public SFX PickupSound;
        internal Image Simple;
        internal Subtexture HUD;
        internal Sprite<int> Animated;
        internal byte Type;

        [Obsolete("Use ArrowInfo.Create or ArrowInfo.CreateAnimated instead")]
        public ArrowInfo()
        {
            Simple = null;
            Animated = null;
            HUD = null;
            Type = 0;
            PickupSound = null;
        }
    #pragma warning disable CS0618

        public static ArrowInfo Create(Image simple) 
        {
            return new ArrowInfo { Simple = simple, Type = 0 };
        }

        public static ArrowInfo Create(Image simple, SFX pickupSound) 
        {
            return new ArrowInfo { Simple = simple, Type = 0, PickupSound = pickupSound };
        }

        public static ArrowInfo CreateAnimated(Sprite<int> animated) 
        {
            return new ArrowInfo { Animated = animated, Type = 1 };
        }

        public static ArrowInfo CreateAnimated(Sprite<int> animated, SFX pickupSound) 
        {
            return new ArrowInfo { Animated = animated, Type = 1, PickupSound = pickupSound};
        }

        public static ArrowInfo Create(Image simple, Subtexture hud) 
        {
            return new ArrowInfo { Simple = simple, Type = 0, HUD = hud };
        }

        public static ArrowInfo CreateAnimated(Sprite<int> animated, Subtexture hud) 
        {
            return new ArrowInfo { Animated = animated, Type = 1, HUD = hud };
        }

        public static ArrowInfo Create(Image simple, Subtexture hud, SFX pickupSound) 
        {
            return new ArrowInfo { Simple = simple, Type = 0, HUD = hud, PickupSound = pickupSound };
        }

        public static ArrowInfo CreateAnimated(Sprite<int> animated, Subtexture hud, SFX pickupSound) 
        {
            return new ArrowInfo { Animated = animated, Type = 1, HUD = hud, PickupSound = pickupSound};
        }
    }
}
