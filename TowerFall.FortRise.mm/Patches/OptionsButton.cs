using System;
using Monocle;
using MonoMod;

namespace TowerFall.Patching
{
    [MonoModPatch("TowerFall.OptionsButton")]
    public class OptionsButton : TowerFall.OptionsButton
    {
        private Wiggler selectedWiggler;


        public OptionsButton(string title) : base(title)
        {
        }

        protected override void OnSelect()
        {
            if (MainMenu is not null)
            {
                MainMenu.TweenUICameraToY(Math.Max(0f, Y - 120f), 10);
            }
            Wiggle();
        }

        public void Wiggle()
        {
            selectedWiggler.Start();
        }
    }
}

namespace TowerFall
{
    internal static class OptionsButtonExt
    {
        extension(Patching.OptionsButton button)
        {
            public void Wiggle()
            {
                button.Wiggle();
            }
        }
    }
}