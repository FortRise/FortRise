using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIModMenu(MainMenu main) : CustomMenuState(main)
{
    private List<UIModPanel> cachedPanels;

    public override void Create()
    {
        ((patch_MainMenu)Main).FilterModOptions = null;

        Main.BackState = MainMenu.MenuState.Main;
        Main.TweenUICameraToY(1);

        var modListPanel = new UIModListPanel(new Vector2(160f, 600));
        modListPanel.OnConfirmed = () =>
        {
            Main.State = ModRegisters.MenuState<UIModToggler>();
        };
        Main.Add(modListPanel);


        var searchPanel = new UISearchPanel(new Vector2(20, 600), new Vector2(20f, 40f));
        searchPanel.OnSearched = (x) =>
        {
            foreach (var p in cachedPanels)
            {
                p.TweenOut();
            }

            cachedPanels.Clear();

            var panels = InitMods(x.ToLowerInvariant(), out int sum);

            if (panels.Count > 0)
            {
                searchPanel.DownItem = panels[0];
            }

            for (int i = 0; i < panels.Count; i += 1)
            {
                var panel = panels[i];

                if (i == 0)
                {
                    panel.UpItem = searchPanel;
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

            cachedPanels = panels;
        };
        Main.Add(searchPanel);

        searchPanel.UpItem = modListPanel;
        modListPanel.DownItem = searchPanel;

        var panels = InitMods("", out int sum);

        if (panels.Count > 0)
        {
            searchPanel.DownItem = panels[0];
        }

        for (int i = 0; i < panels.Count; i += 1)
        {
            var panel = panels[i];

            if (i == 0)
            {
                panel.UpItem = searchPanel;
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

        cachedPanels = panels;
        ((patch_MainMenu)Main).TweenBGCameraToY(2);
        ((patch_MainMenu)Main).ToStartSelected = modListPanel;
    }

    private List<UIModPanel> InitMods(string filter, out int sum)
    {
        var mods = GetMods(filter);

        sum = 60;
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
                    if (RiseCore.UpdateChecks.HasUpdates.Contains(item.Metadata))
                    {
                        panel.HasUpdate = true;
                        modal.AddItem("UPDATE", () =>
                        {
                            UILoader loader = new UILoader();
                            loader.LayerIndex = 0;
                            Main.Add(loader);

                            Task.Run(async () => {
                                var res = await RiseCore.UpdateChecks.DownloadUpdate(item.Metadata);
                                loader.Finish();

                                UIModal modal = new UIModal
                                {
                                    AutoClose = true
                                };
                                modal.LayerIndex = 0;
                                modal.SetTitle("Update Status");

                                if (!res.Check(out _, out string err))
                                {
                                    modal.AddFiller(err);
                                    modal.AddItem("Ok", () => Close());

                                    Main.Add(modal);
                                    Logger.Error(err);
                                    return;
                                }

                                modal.AddFiller("Restart Required!");
                                modal.AddItem("Ok", () => Close());
                                Main.Add(modal);
                                RiseCore.UpdateChecks.HasUpdates.Remove(item.Metadata);
                                panel.HasUpdate = false;
                            });
                        });
                    }

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

        return panels;
    }
    
    public override void Destroy() {}

    private static IReadOnlyList<ModuleMetadata> GetMods(string filter = null) 
    {
        if (string.IsNullOrEmpty(filter))
        {
            return [.. RiseCore.ModuleManager.InternalModuleMetadatas];
        }

        var list = new List<ModuleMetadata>();

        var sorted = new List<(double, ModuleMetadata)>();

        foreach (var metadata in RiseCore.ModuleManager.InternalModuleMetadatas)
        {
            var name = string.IsNullOrEmpty(metadata.DisplayName) ? metadata.Name : metadata.DisplayName;
            var loweredName = name.ToLowerInvariant();

            var score = 1.0 - ((double)LevenshteinDistance(loweredName, filter) / Math.Max(loweredName.Length, filter.Length));

            sorted.Add((score, metadata));
        }

        sorted.Sort((x, y) =>
        {
            if (x.Item1 < y.Item1)
            {
                return 1;
            }

            if (x.Item1 > y.Item1)
            {
                return -1;
            }

            return 0;
        });

        if (sorted.Count == 0)
        {
            return [];
        }

        if (sorted.Count > 1)
        {
            var radius = sorted[0].Item1 * 0.5;

            return [..sorted.Where(x => x.Item1 > radius).Select(x => x.Item2)];
        }

        return [.. sorted.Select(x => x.Item2)];
    }

    private static int LevenshteinDistance(string target, string search)
    {
        if (target == search)
        {
            return 0;
        }

        if (target.Length == 0)
        {
            return search.Length;
        }

        if (search.Length == 0)
        {
            return target.Length;
        }

        int[,] distance = new int[target.Length + 1, search.Length + 1];

        for (int i = 0; i <= target.Length; i += 1)
        {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= search.Length; j += 1)
        {
            distance[0, j] = j;
        }

        for (int i = 1; i <= target.Length; i += 1)
        {
            for (int j = 1; j <= search.Length; j += 1)
            {
                int cost = (target[i - 1] == search[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost
                );
            }
        }

        return distance[target.Length, search.Length];
    }
}
