using System.Threading.Tasks;
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

[CustomMenuState("ModOptions")]
public class UIModOptions : CustomMenuState
{
    public UIModOptions(MainMenu main) : base(main)
    {
    }


    public override void Create()
    {
        var main = (Main as patch_MainMenu);
        if (main.CurrentModule == null) 
        {
            main.State = ModRegisters.MenuState<UIModToggler>();
            main.BackState = ModRegisters.MenuState<UIModMenu>();
            main.TweenUICameraToY(2);
            return;
        }
        var textContainer = new TextContainer(180);

        textContainer.Selected = true;
        if (RiseCore.UpdateChecks.HasUpdates.Contains(main.CurrentModule.Meta))
        {
            var updateButton = new TextContainer.ButtonText("UPDATE");
            updateButton.Pressed(() => {
                UILoader loader = new UILoader();
                main.Add(loader);
                Task.Run(async () => {
                    textContainer.Selected = false;
                    var res = await RiseCore.UpdateChecks.DownloadUpdate(main.CurrentModule.Meta);
                    loader.Finish();

                    UIModal modal = new UIModal
                    {
                        AutoClose = true
                    };
                    modal.SetTitle("Update Status");
                    if (!res.Check(out _, out string err))
                    {
                        modal.AddFiller(err);
                        modal.AddItem("Ok", () => textContainer.Selected = true);
                        main.Add(modal);
                        Logger.Error(err);
                        return;
                    }
                    modal.AddFiller("Restart Required!");
                    modal.AddItem("Ok", () => textContainer.Selected = true);
                    main.Add(modal);
                });
            });
            textContainer.Add(updateButton);
        }

        if (main.CurrentModule.Meta.Update is not null and { GH.Repository: not null })
        {
            var visitGithubButton = new TextContainer.ButtonText("VISIT GITHUB");
            visitGithubButton.Pressed(() => {
                var repo = main.CurrentModule.Meta.Update.GH.Repository;
                RiseCore.UpdateChecks.OpenGithubURL(repo);
            });

            textContainer.Add(visitGithubButton);
        }


        main.CurrentModule.CreateSettings(textContainer);
        main.Add(textContainer);

        main.ToStartSelected = textContainer;
        main.BackState = ModRegisters.MenuState<UIModMenu>();
        main.TweenUICameraToY(2);
    }

    public override void Destroy() {}
}