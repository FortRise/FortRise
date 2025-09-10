using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public class UIModToggler : CustomMenuState
{
    private Dictionary<string, ModuleMetadata> modsMetadata = [];
    private Dictionary<string, bool> onOffs = [];
    private readonly Dictionary<string, TextContainer.Toggleable> toggles = [];
    private HashSet<string> blacklistedMods = [];
    private HashSet<string> oldBlacklistedMods = [];
    public TextContainer Container;

    public UIModToggler(MainMenu main) : base(main)
    {
        Main.Add(Container = new TextContainer(180));
        (Main as patch_MainMenu).ToStartSelected = Container;
    }

    private void ModifyAllButtons(bool enable) 
    {
        foreach (var item in Container.Items) 
        {
            if (item is TextContainer.Toggleable toggleable) 
            {
                if (toggleable.Value == enable)
                    continue;
                toggleable.Value = enable;
                toggleable.WiggleDir = toggleable.Value ? 1 : -1;
                toggleable.OnValueChanged?.Invoke(toggleable.Value);
                toggleable.ValueWiggler.Start();
            }
        }
        if (enable)
            Sounds.ui_subclickOn.Play();
        else
            Sounds.ui_subclickOff.Play();
    }

    public override void Create()
    {
        toggles.Clear();
        Main.BackState = ModRegisters.MenuState<UIModMenu>();

        oldBlacklistedMods = RiseCore.ModuleManager.BlacklistedMods;
        blacklistedMods = [.. oldBlacklistedMods];

        var loadedMetadata = RiseCore.ModuleManager.InternalMods.Select(x => x.Metadata);
        var enableAll = new TextContainer.ButtonText("Enabled All");
        enableAll.Pressed(() => ModifyAllButtons(true));
        var disableAll = new TextContainer.ButtonText("Disable All");
        disableAll.Pressed(() => ModifyAllButtons(false));
        Container.Add(enableAll);
        Container.Add(disableAll);

        string modDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods");
        string[] files = Directory.GetFiles(modDirectory);
        foreach (var file in files) 
        {
            if (!file.EndsWith(".zip"))
            {
                continue;
            }

            var filename = Path.GetFileName(file);

            var metadata = loadedMetadata.Where(meta => meta.PathZip == filename).FirstOrDefault();
            if (metadata == null) 
            {
                using var zipFile = ZipFile.OpenRead(Path.Combine(modDirectory, filename));
                var metaZip = zipFile.GetEntry("meta.json");

                if (metaZip == null)
                {
                    continue;
                }

                using var meta = metaZip.ExtractStream();

                var result = ModuleMetadata.ParseMetadata(file, meta, true);
                if (!result.Check(out metadata, out string error))
                {
                    Logger.Error(error);
                    continue;
                }
            }
            var isBlacklisted = !oldBlacklistedMods.Contains(filename);
            modsMetadata.Add(filename, metadata);
            onOffs.Add(filename, isBlacklisted);
            Container.Add(CreateButton(filename, metadata.Name, onOffs));
        }

        files = Directory.GetDirectories(modDirectory);

        foreach (var dir in files) 
        {
            var folderName = Path.GetFileName(dir);
            var metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath))
            {
                continue;
            }

            var metadata = loadedMetadata.Where(meta => meta.PathDirectory == dir).FirstOrDefault();
            if (metadata == null) 
            {
                var result = ModuleMetadata.ParseMetadata(dir, metaPath);
                if (!result.Check(out metadata, out string error))
                {
                    Logger.Error(error);
                    continue;
                }
            }
            var isBlacklisted = !oldBlacklistedMods.Contains(folderName);
            modsMetadata.Add(folderName, metadata);
            onOffs.Add(folderName, isBlacklisted);
            Container.Add(CreateButton(folderName, metadata.Name, onOffs));
        }
    }

    private TextContainer.Toggleable CreateButton(string physicalName, string modName, Dictionary<string, bool> onOffs) 
    {
        var toggleable = new TextContainer.Toggleable(physicalName, onOffs[physicalName]);

        if (RiseCore.ModuleManager.CantLoad.Contains(modName))
        {
            toggleable.NotSelectedColor = Color.DarkRed;
            toggleable.SelectedColor = Color.Red;
            toggleable.Interactable = false;
            toggleable.OnConfirm = () => {
                Container.Selected = false;
                var uiModal = new UIModal(0);
                uiModal.SetTitle("Missing Mods");
                uiModal.AddFiller("Cannot load mod due to");
                uiModal.AddFiller("Missing dependencies.");
                uiModal.AddFiller("Check fortRiseLog.txt");
                uiModal.AddItem("Ok", () => {
                    Container.Selected = true;
                    uiModal.RemoveSelf();
                });
                uiModal.AutoClose = true;
                Main.Add(uiModal);
            };
        }

        toggleable.Change(on => {
            if (on) 
            {
                RemoveToBlacklist(physicalName, modName);
            }
            else
            {
                AddToBlacklist(physicalName, modName);
            }
        });
        toggles[modName] = toggleable;
        return toggleable;
    }

    private void AddToBlacklist(string physicalName, string modName) 
    {
        if (blacklistedMods.Contains(physicalName))
        {
            return;
        }

        onOffs[physicalName] = false;

        blacklistedMods.Add(physicalName);

        // Check if some mods depend on this mod
        ModuleMetadata modMeta = null;

        foreach (var meta in modsMetadata.Values)
        {
            if (meta.Name == modName)
            {
                modMeta = meta;
            }
        }

        if (modMeta is null)
        {
            return;
        }

        foreach (var mod in modsMetadata.Values) 
        {
            if (mod.Dependencies == null)
            {
                continue;
            }

            foreach (var dep in mod.Dependencies) 
            {
                // we don't wanna blacklist our loader
                if (dep.Name == "FortRise")
                {
                    continue;
                }
                if (modMeta == dep) 
                {
                    string depName = dep.Name;
                    string depPhysName = dep.IsDirectory ? dep.PathDirectory : dep.PathZip;

                    AddToBlacklist(Path.GetFileName(depPhysName), depName);
                    if (toggles.TryGetValue(depName, out var toggle))
                    {
                        toggle.Value = false;
                    }
                }
            }
        }
    }

    private void RemoveToBlacklist(string physicalName, string modName) 
    {
        if (!blacklistedMods.Contains(physicalName))
        {
            return;
        }

        onOffs[physicalName] = true;

        blacklistedMods.Remove(physicalName);

        // Check if some mods depend on this mod
        ModuleMetadata modMeta = null;

        foreach (var meta in modsMetadata.Values)
        {
            if (meta.Name == modName)
            {
                modMeta = meta;
            }
        }

        if (modMeta is null)
        {
            return;
        }

        foreach (var mod in modsMetadata.Values) 
        {
            if (mod.Dependencies == null)
            {
                continue;
            }

            foreach (var dep in modMeta.Dependencies) 
            {
                // we don't wanna blacklist our loader
                if (dep.Name == "FortRise")
                {
                    continue;
                }

                if (modMeta == dep) 
                {
                    string depName = dep.Name;
                    string depPhysName = dep.IsDirectory ? dep.PathDirectory : dep.PathZip;

                    RemoveToBlacklist(Path.GetFileName(depPhysName), depName);
                    if (toggles.TryGetValue(depName, out var toggle))
                    {
                        toggle.Value = true;
                    }
                }
            }
        }
    }

    public override void Destroy()
    {
        foreach (var blacklisted in blacklistedMods)
        {
            RiseCore.logger.LogInformation("Disabling mod: '{blacklistMod}'.", blacklisted);
        }

        bool shouldRestart = !oldBlacklistedMods.SetEquals(blacklistedMods);

        var jsonArray = new List<string>();

        foreach (var blacklisted in blacklistedMods) 
        {
            jsonArray.Add(blacklisted);
        }

        RiseCore.WriteBlacklist(jsonArray, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "blacklist.txt"));
        
        // cleanup
        modsMetadata = null;
        onOffs = null;
        blacklistedMods = null;
        oldBlacklistedMods = null;
        Container = null;
        

        if (shouldRestart) 
        {
            RiseCore.InternalRestart();
            return;
        }
    }
}
