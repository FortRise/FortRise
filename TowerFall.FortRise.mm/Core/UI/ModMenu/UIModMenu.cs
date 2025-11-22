using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIModMenu(MainMenu main) : CustomMenuState(main)
{
    public override void Create()
    {
        ((patch_MainMenu)Main).FilterModOptions = null;
        // if (!string.IsNullOrEmpty(RiseCore.UpdateChecks.UpdateMessage))
        // {
             //var modButton = new TextContainer.ButtonText("UPDATE FORTRISE");
        //     modButton.Pressed(RiseCore.UpdateChecks.OpenFortRiseUpdateURL);
        //     textContainer.Add(modButton);
        // }

        //ModContainer container = new ModContainer(new Vector2(81, 60));
        //ButtonBox toggleMods = new ButtonBox(new Rectangle(81, 39, 128, 18), "TOGGLE MODS", new Vector2(81, 260));
        //toggleMods.OnConfirmed = () => Main.State = ModRegisters.MenuState<UIModToggler>();
        //toggleMods.DownItem = container;
        //Main.Add(toggleMods);
        //container.UpItem = toggleMods;
        //container.OnConfirmed = (meta) => 
        //{
        //    var fortModule = RiseCore.ModuleManager.InternalFortModules
        //        .Where(x => x.Meta is not null)
        //        .Where(x => x.Meta.ToString() == meta.ToString())
        //        .FirstOrDefault();
        //    if (fortModule == null)
        //    {
                //Sounds.ui_invalid.Play(160f, 1f);
        //        return;
        //    }
        //    if (fortModule.GetSettings() == null && !RiseCore.UpdateChecks.HasUpdates.Contains(fortModule.Meta))
        //    {
        //        Sounds.ui_invalid.Play(160f, 1f);
        //        return;
        //    }
        //
        //    lastIndex = container.ScrollYIndex;
        //
        //    (Main as patch_MainMenu).CurrentModule = fortModule;
        //    Main.CanAct = true;
        //    Main.State = ModRegisters.MenuState<UIModOptions>();
        //};

        Main.BackState = MainMenu.MenuState.Main;
        Main.TweenUICameraToY(1);

        var modListPanel = new UIModListPanel(new Vector2(160f, 600));
        Main.Add(modListPanel);

        var mods = GetMods();

        int sum = 60;
        List<UIModPanel> panels = [];

        foreach (var mod in mods)
        {
            var panel = new UIModPanel(
                new(mod),
                new Vector2(20, sum),
                new Vector2(20, 900)
            );

            panel.OnConfirmed = (item) =>
            {
                Main.CanAct = false;
                var fortModule = RiseCore.ModuleManager.InternalFortModules
                    .Where(x => x.Meta is not null)
                    .FirstOrDefault(x => x.Meta.ToString() == item.Metadata.ToString());


                panel.Selected = false;
                var modal = new UIModal
                {
                    LayerIndex = 0
                };
                modal.SetOnBackCallBack(Close);

                if (fortModule is not null)
                {
                    var settings = fortModule.CreateSettings();

                    if (settings is not null)
                    {
                        modal.AddItem("SETTINGS", () =>
                        {
                            Main.CanAct = true;
                            ((patch_MainMenu)Main).FilterModOptions = item.Metadata.Name;
                            Main.State = MainMenu.MenuState.Options;
                        });
                    }
                }

                if (item.Metadata.Update is not null)
                {
                    if (item.Metadata.Update.GH is not null)
                    {
                        modal.AddItem("VISIT GITHUB", () =>
                        {
                            var repo = item.Metadata.Update.GH.Repository;
                            RiseCore.UpdateChecks.OpenGithubURL(repo);
                            Close();
                        });
                    }

                    if (item.Metadata.Update.GB is not null)
                    {
                        modal.AddItem("VISIT GAMEBANANA", () =>
                        {
                            var id = item.Metadata.Update.GB.ID;
                            if (id is { } o)
                            {
                                RiseCore.UpdateChecks.OpenGamebananaURL(o);
                            }
                            Close();
                        });
                    }
                }

                modal.AddItem("CLOSE", () =>
                {
                    Close();
                });

                void Close()
                {
                    Alarm.Set(panel, 1, () => Main.CanAct = true);
                    panel.Selected = true;
                }
                Main.Add(modal);
            };

            sum += 25;

            panels.Add(panel);
        }

        if (panels.Count > 0)
        {
            modListPanel.DownItem = panels[0];
        }

        for (int i = 0; i < panels.Count; i += 1)
        {
            var panel = panels[i];

            if (i == 0)
            {
                panel.UpItem = modListPanel;
            }
            else
            {
                panel.UpItem = panels[i - 1];
            }

            if (i + 1 < panels.Count)
            {
                panel.DownItem = panels[i + 1];    
            }
        }

        Main.Add(panels);
        Main.MaxUICameraY = sum;
        ((patch_MainMenu)Main).TweenBGCameraToY(2);
        ((patch_MainMenu)Main).ToStartSelected = modListPanel;
    }

    public override void Destroy() {}

    private static IReadOnlyList<ModuleMetadata> GetMods() 
    {
        return [.. RiseCore.ModuleManager.InternalModuleMetadatas];
    }
}
