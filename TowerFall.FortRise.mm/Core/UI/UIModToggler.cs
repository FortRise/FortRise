using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
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

    public override void OnEnter()
    {
        oldBlacklistedMods = RiseCore.Loader.BlacklistedMods;
        var loadedMetadata = RiseCore.InternalMods.Select(x => x.Metadata);
        var loadedMods = new List<OptionsButton>();
        string[] files = Directory.GetFiles("Mods");
        foreach (var file in files) 
        {
            if (!file.EndsWith(".zip"))
                continue;
            var filename = Path.GetFileName(file);

            var metadata = loadedMetadata.Where(meta => meta.PathZip == file).FirstOrDefault();
            if (metadata == null) 
            {
                using var zipFile = ZipFile.Read(Path.Combine("Mods", filename));

                string metaPath = null;
                if (zipFile.ContainsEntry("meta.json")) 
                    metaPath = "meta.json";
                else if (zipFile.ContainsEntry("meta.xml")) 
                    metaPath = "meta.xml";
                else 
                    return;

                using var meta = zipFile[metaPath].ExtractStream();
                // TODO xml
                metadata = RiseCore.ParseMetadataWithJson(file, meta, true);
            }
            var isBlacklisted = !oldBlacklistedMods.Contains(file);
            modsMetadata.Add(file, metadata);
            onOffs.Add(file, isBlacklisted);
            loadedMods.Add(CreateButton(file, onOffs));
        }

        files = Directory.GetDirectories("Mods");
        foreach (var dir in files) 
        {
            // TODO xml
            var metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath)) 
                continue;
            var metadata = loadedMetadata.Where(meta => meta.PathDirectory == dir).FirstOrDefault();
            if (metadata == null) 
            {
                metadata = RiseCore.ParseMetadataWithJson(dir, metaPath);
            }
            var isWhitelisted = !oldBlacklistedMods.Contains(dir);
            modsMetadata.Add(dir, metadata);
            onOffs.Add(dir, isWhitelisted);
            loadedMods.Add(CreateButton(dir, onOffs));
        }

        for (int i = 0; i < loadedMods.Count; i++) 
        {
            var modButton = loadedMods[i];
            modButton.TweenTo = new Vector2(200f, 45 + i * 12);
            modButton.Position = modButton.TweenFrom = new Vector2((~i & 1) != 0 ? -160 : 480, 45 + i * 12);
            if (i < 0)
                modButton.UpItem = loadedMods[i - 1];
            if (i < loadedMods.Count - 1)
                modButton.DownItem = loadedMods[i + 1];
        }
        Scene.Add(loadedMods);
    }

    private OptionsButton CreateButton(string modName, Dictionary<string, bool> onOffs) 
    {
        var optionButton = new OptionsButton(modName.ToUpperInvariant());
        optionButton.SetCallbacks(() => {
            optionButton.State = onOffs[modName] ? "ON" : "OFF";
        }, null, null, () => {
            onOffs[modName] = !onOffs[modName];
            return onOffs[modName];
        });
        return optionButton;
    }

    private void AddToBlacklist(string modName) 
    {
        if (blacklistedMods.Contains(modName))
            return;
        onOffs[modName] = false;

        blacklistedMods.Add(modName);

        // TODO disable mods that depend on this mod
    }

    private void RemoveToBlacklist(string modName) 
    {
        if (!blacklistedMods.Contains(modName))
            return;
        
        blacklistedMods.Remove(modName);
    }

    public override void OnLeave()
    {
        bool shouldRestart = !oldBlacklistedMods.SetEquals(blacklistedMods);
        
        // cleanup
        modsMetadata = null;
        onOffs = null;
        blacklistedMods = null;
        oldBlacklistedMods = null;

        if (shouldRestart) 
            return;
        
    }
}