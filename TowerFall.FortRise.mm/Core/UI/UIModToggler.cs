using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace FortRise;

public class UIModToggler : FortRiseUI
{
    private Dictionary<string, ModuleMetadata> modsMetadata = new();
    private Dictionary<string, bool> onOffs = new();
    private HashSet<string> blacklistedMods = new();
    private HashSet<string> oldBlacklistedMods = new();
    public TextContainer Container;

    private Scene scene;

    public UIModToggler(Scene scene) 
    {
        this.scene = scene;
        scene.Add(Container = new TextContainer(180));
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

    public override void OnEnter()
    {
        oldBlacklistedMods = RiseCore.Loader.BlacklistedMods;
        blacklistedMods = new HashSet<string>(oldBlacklistedMods);
        var loadedMetadata = RiseCore.InternalMods.Select(x => x.Metadata);
        var enableAll = new TextContainer.ButtonText("Enabled All");
        enableAll.Pressed(() => ModifyAllButtons(true));
        var disableAll = new TextContainer.ButtonText("Disable All");
        disableAll.Pressed(() => ModifyAllButtons(false));
        Container.Add(enableAll);
        Container.Add(disableAll);
        string[] files = Directory.GetFiles("Mods");
        foreach (var file in files) 
        {
            if (!file.EndsWith(".zip"))
                continue;
            var filename = Path.GetFileName(file);

            var metadata = loadedMetadata.Where(meta => meta.PathZip == filename).FirstOrDefault();
            if (metadata == null) 
            {
                using var zipFile = ZipFile.OpenRead(Path.Combine("Mods", filename));
                var metaZip = zipFile.GetEntry("meta.json");

                if (metaZip == null)
                {
                    continue;
                }

                using var meta = metaZip.ExtractStream();
                metadata = ModuleMetadata.ParseMetadata(file, meta, true);
            }
            var isBlacklisted = !oldBlacklistedMods.Contains(filename);
            modsMetadata.Add(filename, metadata);
            onOffs.Add(filename, isBlacklisted);
            Container.Add(CreateButton(filename, onOffs));
        }

        files = Directory.GetDirectories("Mods");

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
                metadata = ModuleMetadata.ParseMetadata(dir, metaPath);
            }
            var isWhitelisted = !oldBlacklistedMods.Contains(folderName);
            modsMetadata.Add(folderName, metadata);
            onOffs.Add(folderName, isWhitelisted);
            Container.Add(CreateButton(folderName, onOffs));
        }
    }

    private TextContainer.Toggleable CreateButton(string modName, Dictionary<string, bool> onOffs) 
    {
        var toggleable = new TextContainer.Toggleable(modName, onOffs[modName]);
        if (RiseCore.Loader.CantLoad.Contains(modName))
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
                scene.Add(uiModal);
            };
        }
        toggleable.Change(on => {
            if (on) 
                RemoveToBlacklist(modName);
            else
                AddToBlacklist(modName);
        });
        return toggleable;
    }

    private void AddToBlacklist(string modName) 
    {
        if (blacklistedMods.Contains(modName))
            return;
        onOffs[modName] = false;

        blacklistedMods.Add(modName);

        var modmeta = modsMetadata.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x.PathZip) && x.PathZip.Replace("Mods/", "") == modName)
            .FirstOrDefault();
        if (modmeta == null)
            modmeta = modsMetadata.Select(x => x.Value).Where(x => x.PathDirectory.Replace("Mods\\", "") == modName).FirstOrDefault();

        
        if (modmeta == null)
            return;

        foreach (var mod in modsMetadata.Values) 
        {
            if (mod.Dependencies == null)
                continue;
            foreach (var dep in mod.Dependencies) 
            {
                if (modmeta == dep) 
                {
                    string depName;
                    if (!string.IsNullOrEmpty(mod.PathZip)) 
                    {
                        depName = Path.GetFileName(mod.PathZip);
                    }
                    else 
                    {
                        var folderName = Path.GetFileName(mod.PathDirectory);
                        depName = folderName;
                    }
                    AddToBlacklist(depName);
                    var item = Container.Items.Where(x => x is TextContainer.Toggleable toggle && toggle.Text == depName.ToUpperInvariant()).Cast<TextContainer.Toggleable>().FirstOrDefault();
                    item.Value = false;
                }
            }
        }
    }

    private void RemoveToBlacklist(string modName) 
    {
        if (!blacklistedMods.Contains(modName))
            return;
        
        blacklistedMods.Remove(modName);

        var modmeta = modsMetadata.Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x.PathZip) && x.PathZip.Replace("Mods/", "") == modName)
            .FirstOrDefault();
        if (modmeta == null)
            modmeta = modsMetadata.Select(x => x.Value).Where(x => x.PathDirectory.Replace("Mods\\", "") == modName).FirstOrDefault();

        
        if (modmeta is null || modmeta.Dependencies is null)
            return;
        
        foreach (var mod in modsMetadata.Values) 
        {
            foreach (var dep in modmeta.Dependencies)
            {
                if (mod == dep) 
                {
                    string depName;
                    if (!string.IsNullOrEmpty(mod.PathZip)) 
                    {
                        depName = Path.GetFileName(mod.PathZip);
                    }
                    else 
                    {
                        var folderName = Path.GetFileName(mod.PathDirectory);
                        depName = folderName;
                    }
                    RemoveToBlacklist(depName);
                    var item = Container.Items.Where(x => x is TextContainer.Toggleable toggle && toggle.Text == depName.ToUpperInvariant()).Cast<TextContainer.Toggleable>().FirstOrDefault();
                    item.Value = true;
                }
            }
        }
    }

    public override void OnLeave()
    {
        bool shouldRestart = !oldBlacklistedMods.SetEquals(blacklistedMods);

        var jsonArray = new List<string>();

        foreach (var blacklisted in blacklistedMods) 
        {
            jsonArray.Add(blacklisted);
        }
        RiseCore.WriteBlacklist(jsonArray, "Mods/blacklist.txt");
        
        // cleanup
        modsMetadata = null;
        onOffs = null;
        blacklistedMods = null;
        oldBlacklistedMods = null;
        Container = null;
        scene = null;
        

        if (shouldRestart) 
        {
            RiseCore.InternalRestart();
            return;
        }
    }
}