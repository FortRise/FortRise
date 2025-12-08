using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

internal class ModOptionsButton : TowerFall.Patching.OptionsButton
{
    public string ModName;
    public string PhysicalName;

    public ModOptionsButton(string title) : base(title)
    {
    }
}

public class UIModToggler : CustomMenuState
{
    private Dictionary<string, ModuleMetadata> modsMetadata;
    private HashSet<string> oldBlacklistedMods;
    private HashSet<string> blacklistedMods;
    private List<ModOptionsButton> buttons;
    private Dictionary<string, ModOptionsButton> toggles;
    private Dictionary<string, bool> onOffs;

    public UIModToggler(MainMenu main) : base(main)
    {
    }

    public override void Create()
    {
        modsMetadata = [];
        toggles = [];
        buttons = [];
        onOffs = [];
        Main.BackState = ModRegisters.MenuState<UIModMenu>();
        oldBlacklistedMods = RiseCore.ModuleManager.BlacklistedMods;
        blacklistedMods = [.. oldBlacklistedMods];

        var loadedMetadata = RiseCore.ModuleManager.InternalMods.Select(x => x.Metadata);
        var enableAll = new OptionsButton("ENABLE ALL");
        enableAll.SetCallbacks(
            () => enableAll.State = string.Empty,
            null,
            null,
            () =>
            {
                ModifyAllButtons(true);
                return true;
            }
        );

        int sum = 0;

        enableAll.TweenTo = new Vector2(200f, 45 + sum * 12);
        enableAll.Position = enableAll.TweenFrom = new Vector2((sum % 2 == 0) ? (-160) : 480, 45 + sum * 12);

        var disableAll = new OptionsButton("DISABLE ALL");
        disableAll.SetCallbacks(
            () => disableAll.State = string.Empty,
            null,
            null,
            () =>
            {
                ModifyAllButtons(false);
                return true;
            }
        );

        sum += 1;

        disableAll.TweenTo = new Vector2(200f, 45 + sum * 12);
        disableAll.Position = disableAll.TweenFrom = new Vector2((sum % 2 == 0) ? (-160) : 480, 45 + sum * 12);

        sum += 1;

        enableAll.DownItem = disableAll;
        disableAll.UpItem = enableAll;

        Main.Add(enableAll);
        Main.Add(disableAll);

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
            var button = CreateButton(filename, metadata.Name, onOffs);
            if (button is not null)
            {
                button.TweenTo = new Vector2(200f, 45 + sum * 12);
                button.Position = button.TweenFrom = new Vector2((sum % 2 == 0) ? (-160) : 480, 45 + sum * 12);
                buttons.Add(button);
                sum += 1;
            }
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
            var button = CreateButton(folderName, metadata.Name, onOffs);
            if (button is not null)
            {
                button.TweenTo = new Vector2(200f, 45 + sum * 12);
                button.Position = button.TweenFrom = new Vector2((sum % 2 == 0) ? (-160) : 480, 45 + sum * 12);
                buttons.Add(button);
                sum += 1;
            }
        }

        if (buttons.Count > 0)
        {
            disableAll.DownItem = buttons[0];
        }

        for (int i = 0; i < buttons.Count; i += 1)
        {
            var panel = buttons[i];

            if (i == 0)
            {
                panel.UpItem = disableAll;
            }
            else
            {
                panel.UpItem = buttons[i - 1];
            }

            if (i + 1 < buttons.Count)
            {
                panel.DownItem = buttons[i + 1];    
            }
        }

        Main.MaxUICameraY = 45 + sum * 12;

        ((patch_MainMenu)Main).TweenBGCameraToY(2);
        ((patch_MainMenu)Main).ToStartSelected = enableAll;
        Main.Add(buttons);
    }

    public override void Destroy()
    {
        bool shouldRestart = !oldBlacklistedMods.SetEquals(blacklistedMods);

        var jsonArray = new List<string>();

        foreach (var blacklisted in blacklistedMods) 
        {
            jsonArray.Add(blacklisted);
        }

        RiseCore.WriteBlacklist(jsonArray, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "blacklist.txt"));

        foreach (var blacklisted in blacklistedMods)
        {
            RiseCore.logger.LogInformation("Disabled: '{blacklistMod}'.", blacklisted);
        }
        
        // cleanup
        modsMetadata = null;
        onOffs = null;
        blacklistedMods = null;
        oldBlacklistedMods = null;
        

        if (shouldRestart) 
        {
            RiseCore.InternalRestart();
            return;
        }
    }

    private void ModifyAllButtons(bool enable) 
    {
        foreach (var toggleable in buttons) 
        {
            if (enable)
            {
                toggleable.State = "ON";
                RemoveToBlacklist(toggleable.PhysicalName, toggleable.ModName);
            }
            else
            {
                toggleable.State = "OFF";
                AddToBlacklist(toggleable.PhysicalName, toggleable.ModName);
            }
            toggleable.Wiggle();
        }

        if (enable)
        {
            Sounds.ui_subclickOn.Play();
        }
        else
        {
            Sounds.ui_subclickOff.Play();
        }
    }

    private ModOptionsButton CreateButton(string physicalName, string modName, Dictionary<string, bool> onOffs) 
    {
        var toggleable = new ModOptionsButton(physicalName.ToUpperInvariant())
        {
            PhysicalName = physicalName,
            ModName = modName
        };


        if (RiseCore.ModuleManager.CantLoad.Contains(modName))
        {
            return null;
        }

        toggleable.SetCallbacks(() =>
        {
            toggleable.State = toggleable.State == "ON" ? "OFF" : "ON";
            if (onOffs[physicalName])
            {
                toggleable.State = "ON";
                RemoveToBlacklist(physicalName, modName);
            }
            else
            {
                toggleable.State = "OFF";
                AddToBlacklist(physicalName, modName);
            }
        }, null, null, () =>
        {
            ref var onOff = ref CollectionsMarshal.GetValueRefOrNullRef(onOffs, physicalName);
            if (Unsafe.IsNullRef(ref onOff))
            {
                return false;
            }

            return onOff = !onOff;
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
                        toggle.Wiggle();
                        toggle.State = "OFF";
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
                        toggle.Wiggle();
                        toggle.State = "ON";
                    }
                }
            }
        }
    }
}