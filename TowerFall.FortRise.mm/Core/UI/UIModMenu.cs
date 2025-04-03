using FortRise.Adventure;
using TowerFall;

namespace FortRise;

[CustomMenuState("Mods")]
public class UIModMenu : CustomMenuState
{
    public UIModMenu(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        var textContainer = new TextContainer(160);
        var toggleButton = new TextContainer.ButtonText("Toggle Mods");
        toggleButton.Pressed(() => {
            Main.State = ModRegisters.MenuState<UIModOptions>();
        });
        textContainer.Add(toggleButton);

        if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
        {
            var modButton = new TextContainer.ButtonText("UPDATE FORTRISE");
            modButton.Pressed(RiseCore.UpdateChecks.OpenFortRiseUpdateURL);
            textContainer.Add(modButton);
        }

        foreach (var mod in FortRise.RiseCore.InternalFortModules) 
        {
            var version = mod.Meta.Version.ToString();
            var setupName = mod.Meta.Name + " v" + version.ToUpperInvariant();
            string author = mod.Meta.Author ?? "";

            string title;
            if (string.IsNullOrEmpty(author))
            {
                title = setupName.ToUpperInvariant();
            }
            else 
            {
                title = setupName.ToUpperInvariant() + " - " + author.ToUpperInvariant();
            }

            bool hasUpdate = RiseCore.UpdateChecks.HasUpdates.Contains(mod.Meta);

            if (hasUpdate)
            {
                title += "<t=variants/newVariantsTagSmall>";
            }

            var modButton = new IconButtonText(title);
            if (mod is not AdventureModule or NoModule) 
            {
                modButton.Pressed(() => {
                    Main.State = ModRegisters.MenuState<UIModOptions>();
                    (Main as patch_MainMenu).CurrentModule = mod;
                });
            }

            textContainer.Add(modButton);
        }
        Main.Add(textContainer);
        (Main as patch_MainMenu).ToStartSelected = textContainer;

        Main.BackState = MainMenu.MenuState.Main;
        Main.TweenUICameraToY(1);
    }

    public override void Destroy()
    {
        if ((Main as patch_MainMenu).switchTo != ModRegisters.MenuState<UIModOptions>())
        {
            Main.SaveOnTransition = true;
            foreach (var mod in FortRise.RiseCore.InternalFortModules) 
            {
                if (mod.InternalSettings == null)
                    continue;
                mod.SaveSettings();
            }
        }
    }
}
