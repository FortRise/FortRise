using System.Threading.Tasks;
using TowerFall;

namespace FortRise;

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


        var settings = main.CurrentModule.GetSettings();
        if (settings is not null)
        {
            settings.Create(new SettingsCreate(textContainer));
        }
        main.Add(textContainer);

        main.ToStartSelected = textContainer;
        main.BackState = ModRegisters.MenuState<UIModMenu>();
        main.TweenUICameraToY(2);
    }

    public override void Destroy() {}
}
