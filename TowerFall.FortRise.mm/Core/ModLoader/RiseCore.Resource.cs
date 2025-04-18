using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Monocle;
using TowerFall;

namespace FortRise;


public partial class RiseCore
{
    public sealed class ResourceTypeFile {}
    public sealed class ResourceTypeFolder {}

    public sealed class ResourceTypeAssembly {}
    public sealed class ResourceTypeXml {}
    public sealed class ResourceTypeJson {}
    public sealed class ResourceTypeOel {}
    public sealed class ResourceTypeQuestTowerFolder {}
    public sealed class ResourceTypeDarkWorldTowerFolder {}
    public sealed class ResourceTypeVersusTowerFolder {}
    public sealed class ResourceTypeTrialsTowerFolder {}
    public sealed class ResourceTypeAtlas {}
    public sealed class ResourceTypeAtlasPng {}
    public sealed class ResourceTypeMenuAtlasPng {}
    public sealed class ResourceTypeBGAtlasPng {}
    public sealed class ResourceTypeBossAtlasPng {}
    public sealed class ResourceTypeSpriteData {}
    public sealed class ResourceTypeGameData {}
    public sealed class ResourceTypeWaveBank {}
    public sealed class ResourceTypeSoundBank {}
    public sealed class ResourceTypeXMLSoundBank {}
    public sealed class ResourceTypeJSONSoundBank {}
    public sealed class ResourceTypeWavFile {}
    public sealed class ResourceTypeOggFile {}
    public sealed class ResourceTypeAudioEngine {}
    public sealed class ResourceTypeEffects {}

    internal static HashSet<string> BlacklistedExtension = [
        ".csproj", ".cs", ".md", ".toml", ".aseprite", ".ase", ".xap"
    ];

    internal static HashSet<string> BlacklistedRootFolders = [
        "bin", "obj"
    ];

    internal static HashSet<string> BlacklistedCommonFolders = [
        "packer/"
    ];

    private static readonly char[] SplitSeparator = ['/'];


    /// <summary>
    /// A class that contains a path and stream to your resource works both on folder and zip.
    /// </summary>
    public abstract class Resource : IResourceInfo
    {
        public string FullPath;
        public string Path;
        public string Root;
        public string RootPath => Root + Path;
        public List<IResourceInfo> Childrens = new();
        public ModResource Source;
        public Type ResourceType;

        public abstract Stream Stream { get; }

        string IResourceInfo.FullPath
        {
            get => FullPath;
            set => FullPath = value;
        } 

        string IResourceInfo.Path 
        {
            get => Path;
            set => Path = value;
        } 

        string IResourceInfo.Root 
        {
            get => Root;
            set => Root = value;
        } 

        IReadOnlyList<IResourceInfo> IResourceInfo.Childrens => Childrens;

        IModResource IResourceInfo.Resource => Source;

        Type IResourceInfo.ResourceType => ResourceType;

        public string Text => ModIO.ReadAllText(this);
        
#nullable enable
        public XmlDocument? Xml
        {
            get 
            {
                try 
                {
                    var xml = ModIO.LoadXml(this);
                    return xml;
                }
                catch (XmlException ex)
                {
                    Logger.Error(ex.ToString());
                    return null;
                }
            }
        }
#nullable disable

        public Resource(ModResource resource, string path, string fullPath)
        {
            FullPath = fullPath;
            Path = path;
            Source = resource;
        }

        public IResourceInfo GetRelativePath(string path)
        {
            string actualPath = System.IO.Path.Combine(RootPath, path);
            return RiseCore.ResourceTree.Get(actualPath);
        }

        public bool ExistsRelativePath(string path)
        {
            string actualPath = System.IO.Path.Combine(RootPath, path);
            return RiseCore.ResourceTree.IsExist(actualPath);
        }

        public void AssignType()
        {
            var path = Path;
            var filename = System.IO.Path.GetFileName(path);


            if (path.StartsWith("Content/Atlas/atlas/") && filename.EndsWith(".png"))
            {
                ResourceType = typeof(ResourceTypeAtlasPng);
            }
            else if (path.StartsWith("Content/Atlas/menuAtlas/") && filename.EndsWith(".png"))
            {
                ResourceType = typeof(ResourceTypeMenuAtlasPng);
            }
            else if (path.StartsWith("Content/Atlas/bossAtlas/") && filename.EndsWith(".png"))
            {
                ResourceType = typeof(ResourceTypeBossAtlasPng);
            }
            else if (path.StartsWith("Content/Atlas/bgAtlas/") && filename.EndsWith(".png"))
            {
                ResourceType = typeof(ResourceTypeBGAtlasPng);
            }
            else if (path.StartsWith("Content/Atlas") && filename.EndsWith(".png"))
            {
                foreach (var ext in AtlasReader.InternalReaders.Keys)
                {
                    if (ResourceTree.IsExist(this, path.Replace(".png", ext)))
                    {
                        ResourceType = typeof(ResourceTypeAtlas);
                    }
                }

            }
            else if (path.EndsWith(".dll"))
            {
                ResourceType = typeof(ResourceTypeAssembly);
            }
            else if (path.StartsWith("Content/Atlas/GameData") && filename.EndsWith(".xml"))
            {
                ResourceType = typeof(ResourceTypeGameData);
            }
            else if (path.StartsWith("Content/Atlas/SpriteData") && filename.EndsWith(".xml"))
            {
                ResourceType = typeof(ResourceTypeSpriteData);
            }
            else if (path.StartsWith("Content/Levels/DarkWorld"))
            {
                if (ResourceTree.IsExist(this, path + "/tower.xml"))
                {
                    ResourceType = typeof(ResourceTypeDarkWorldTowerFolder);
                }
                else AssignLevelFile();
            }
            else if (path.StartsWith("Content/Levels/Versus"))
            {
                if (ResourceTree.IsExist(this, path + "/tower.xml"))
                {
                    ResourceType = typeof(ResourceTypeVersusTowerFolder);
                }
                else AssignLevelFile();
            }
            else if (path.StartsWith("Content/Levels/Quest"))
            {
                if (ResourceTree.IsExist(this, path + "/tower.xml"))
                {
                    ResourceType = typeof(ResourceTypeQuestTowerFolder);
                }
                else AssignLevelFile();
            }
            else if (path.StartsWith("Content/Levels/Trials"))
            {
                if (ResourceTree.IsExist(this, path + "/tower.xml"))
                {
                    ResourceType = typeof(ResourceTypeTrialsTowerFolder);
                }
                else AssignLevelFile();
            }
            else if (path.StartsWith("Content/Music"))
            {
                if (path.EndsWith(".xgs"))
                {
                    ResourceType = typeof(ResourceTypeAudioEngine);
                }
                else if (path.EndsWith(".xsb"))
                {
                    ResourceType = typeof(ResourceTypeSoundBank);
                }
                else if (path.EndsWith("xwb"))
                {
                    ResourceType = typeof(ResourceTypeWaveBank);
                }
                else if (path.EndsWith(".wav"))
                {
                    ResourceType = typeof(ResourceTypeWavFile);
                }
                else if (path.EndsWith(".ogg"))
                {
                    ResourceType = typeof(ResourceTypeOggFile);
                }
                // FIXME fix normal file
                else
                {
                    ResourceType = typeof(ResourceTypeFile);
                }
            }
            else if (path.StartsWith("Content/SFX"))
            {
                if (path.EndsWith(".wav"))
                {
                    ResourceType = typeof(ResourceTypeWavFile);
                }
                else if (path.EndsWith(".ogg"))
                {
                    ResourceType = typeof(ResourceTypeOggFile);
                }
                else if (path.EndsWith("SoundBank.xml"))
                {
                    ResourceType = typeof(ResourceTypeXMLSoundBank);
                }
                else if (path.EndsWith("SoundBank.json"))
                {
                    ResourceType = typeof(ResourceTypeJSONSoundBank);
                }
                // FIXME fix normal file
                else
                {
                    ResourceType = typeof(ResourceTypeFile);
                }
            }
            else if (path.EndsWith(".xml"))
            {
                ResourceType = typeof(ResourceTypeXml);
            }
            else if (path.EndsWith(".fxb"))
            {
                ResourceType = typeof(ResourceTypeEffects);
            }
            else if (Childrens.Count != 0)
            {
                ResourceType = typeof(ResourceTypeFolder);
            }
            else
            {
                ResourceType = typeof(ResourceTypeFile);
            }
            RiseCore.Events.Invoke_OnResourceAssignType(path, filename, ref ResourceType);

            void AssignLevelFile()
            {
                if (path.EndsWith(".json"))
                {
                    ResourceType = typeof(ResourceTypeJson);
                }
                
                else if (path.EndsWith(".oel"))
                {
                    ResourceType = typeof(ResourceTypeOel);
                }
            }
        }
    }

    public class FileResource : Resource
    {
        public FileResource(ModResource resource, string path, string fullPath) : base(resource, path, fullPath)
        {
        }

        public override Stream Stream
        {
            get
            {
                if (!File.Exists(FullPath))
                    return null;
                return File.OpenRead(FullPath);
            }
        }
    }

    public class ZipResource : Resource
    {
        public ZipArchiveEntry Entry;

        public ZipResource(ModResource resource, string path, string fullPath, ZipArchiveEntry entry) : base(resource, path, fullPath)
        {
            Entry = entry;
        }

        public override Stream Stream
        {
            get 
            {
                ZipModResource modSource = (ZipModResource)Source;
                var entry = modSource.Zip.GetEntry(Path);
                if (entry == null) 
                {
                    throw new KeyNotFoundException($"File {Path} not found in archive {modSource.Metadata.PathZip}");
                }
                return entry.ExtractStream();
            } 
        }
    }

    public abstract class ModResource : IModResource
    {
        public ModuleMetadata Metadata;
        public FortContent Content;
        public Dictionary<string, IResourceInfo> Resources = new();
        private bool disposedValue;

        ModuleMetadata IModResource.Metadata => Metadata;
        FortContent IModResource.Content => Content;
        public Dictionary<string, IResourceInfo> OwnedResources => Resources;


        public ModResource(ModuleMetadata metadata)
        {
            Metadata = metadata;
            Content = new FortContent(this);
        }

        public ModResource()
        {
            Content = new FortContent(this);
        }


        public void Add(string path, IResourceInfo resource)
        {
            var rootName = (Metadata is not null ? Metadata.Name : "::global::");
            var rootPath = resource.Root = $"mod:{rootName}/";

            Logger.Verbose("[RESOURCE] Loaded On:" + rootPath);
            Logger.Verbose("[RESOURCE] Loaded:" + path);
            if (Resources.ContainsKey(path))
                return;


            Resources.Add(path, resource);
            RiseCore.ResourceTree.TreeMap.Add($"{rootPath}{path}", resource);
        }


        public abstract void Lookup(string prefix);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeInternal();
                }

                disposedValue = true;
            }
        }

        internal virtual void DisposeInternal() {}

        ~ModResource()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ZipModResource : ModResource
    {
        public ZipArchive Zip;


        public ZipModResource(ModuleMetadata metadata) : base(metadata)
        {
            Zip = ZipFile.OpenRead(metadata.PathZip);
        }

        internal override void DisposeInternal()
        {
            Zip.Dispose();
        }


        public override void Lookup(string prefix)
        {
            var folders = new Dictionary<string, ZipResource>();

            var entries = Zip.Entries.OrderBy(f => f.FullName);

            foreach (var entry in entries)
            {
                var fileName = entry.FullName.Replace('\\', '/');

                if (entry.IsEntryDirectory())
                {
                    if (BlacklistedCommonFolders.Contains(fileName))
                        continue;
                    var file = fileName.Remove(fileName.Length - 1);
                    var zipResource = new ZipResource(this, file, prefix + file, entry);
                    Add(file, zipResource);
                    folders.Add(file, zipResource);
                    var split = file.Split(SplitSeparator);
                    Array.Resize(ref split, split.Length - 1);
                    var newPath = CombineAllPath(split);
                    if (folders.TryGetValue(newPath, out var resource))
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }
                else
                {
                    if (BlacklistedExtension.Contains(Path.GetExtension(fileName)))
                        continue;
                    var zipResource = new ZipResource(this, fileName, prefix + fileName, entry);
                    Add(fileName, zipResource);
                    if (folders.TryGetValue(Path.GetDirectoryName(fileName).Replace('\\', '/'), out var resource))
                    {
                        resource.Childrens.Add(zipResource);
                    }
                }
            }
        }

        private static string CombineAllPath(string[] paths)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                sb.Append(path);
                if (i != paths.Length - 1)
                    sb.Append('/');
            }
            return sb.ToString();
        }
    }

    public class FolderModResource : ModResource
    {
        public string FolderDirectory;
        public FolderModResource(ModuleMetadata metadata) : base(metadata)
        {
            FolderDirectory = metadata.PathDirectory.Replace('\\', '/');
        }

        public FolderModResource(string path) : base()
        {
            FolderDirectory = path.Replace('\\', '/');
        }

        public override void Lookup(string prefix)
        {
            var files = Directory.GetFiles(FolderDirectory);
            Array.Sort(files);
            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath)))
                    continue;
                var simplifiedPath = filePath.Replace(FolderDirectory + '/', "");
                var fileResource = new FileResource(this, simplifiedPath, filePath);
                Add(simplifiedPath, fileResource);
            }
            var folders = Directory.GetDirectories(FolderDirectory);
            Array.Sort(folders);
            foreach (var folder in folders)
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(FolderDirectory + '/', "");
                if (BlacklistedCommonFolders.Contains(simpliPath))
                    continue;
                var newFolderResource = new FileResource(this, simpliPath, fixedFolder);
                Lookup(prefix, folder, FolderDirectory, newFolderResource);
                Add(simpliPath, newFolderResource);
            }
        }

        public void Lookup(string prefix, string path, string modDirectory, FileResource folderResource)
        {
            var files = Directory.GetFiles(path);
            Array.Sort(files);
            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i].Replace('\\', '/');
                if (BlacklistedExtension.Contains(Path.GetExtension(filePath)))
                    continue;
                var simplifiedPath = filePath.Replace(modDirectory + '/', "");
                var fileResource = new FileResource(this, simplifiedPath, filePath);
                Add(simplifiedPath, fileResource);
                folderResource.Childrens.Add(fileResource);
            }
            var folders = Directory.GetDirectories(path);
            Array.Sort(folders);
            foreach (var folder in folders)
            {
                var fixedFolder = folder.Replace('\\', '/');
                var simpliPath = fixedFolder.Replace(modDirectory + '/', "");
                if (BlacklistedCommonFolders.Contains(simpliPath))
                    continue;
                var newFolderResource = new FileResource(this, simpliPath, prefix + simpliPath);
                Lookup(prefix, folder, modDirectory, newFolderResource);
                Add(simpliPath, newFolderResource);
                folderResource.Childrens.Add(newFolderResource);
            }
        }
    }

    public static class ResourceTree
    {
        public static Dictionary<string, IResourceInfo> TreeMap = new();
        public static List<IModResource> ModResources = new();


        public static void AddMod(ModuleMetadata metadata, IModResource resource)
        {
            var name = (metadata is not null ? metadata.Name : "::global::");
            var prefixPath = $"mod:{name}/";

            if (TreeMap.ContainsKey(prefixPath))
            {
                Logger.Warning($"[RESOURCE] Conflicting mod asset name found: {prefixPath}");
                return;
            }
            resource.Lookup(prefixPath);

            Logger.Info($"[RESOURCE] Initializing {resource.Metadata} resources...");
            Initialize(resource);
            ModResources.Add(resource);
        }

        internal static void Initialize(IModResource resource)
        {
            foreach (var res in resource.OwnedResources.Values)
            {
                res.AssignType();
            }
        }

        public static IResourceInfo Get(string path)
        {
            TreeMap.TryGetValue(path.Replace('\\', '/'), out var res);
            if (res == null)
            {
                throw new Exception($"Resource path: '{path}' not found or does not exists.");
            }

            return res;
        }

        public static bool TryGetValue(string path, out IResourceInfo res)
        {
            return TreeMap.TryGetValue(path.Replace('\\', '/'), out res);
        }

        public static bool IsExist(string path)
        {
            return TreeMap.ContainsKey(path.Replace('\\', '/'));
        }

        public static bool IsExist(IResourceInfo resource, string path)
        {
            return TreeMap.ContainsKey((resource.Root + path).Replace('\\', '/'));
        }

        // If you were expecting this feature, please use the RiseCore.Events.OnAfterModdedLoadContent event instead.
        internal static void AfterModdedLoadContent()
        {
            foreach (var mod in ModResources)
            {
                if (mod.Content == null)
                    continue;
                Events.Invoke_OnAfterModdedLoadContent(mod.Content);
            }
        }

        public static async Task DumpAll()
        {
            try
            {
                if (!Directory.Exists("DUMP"))
                    Directory.CreateDirectory("DUMP");
                using var file = File.Create("DUMP/resourcedump.txt");
                using TextWriter tw = new StreamWriter(file);

                tw.WriteLine("FORTRISE RESOURCE DUMP");
                tw.WriteLine("VERSION 5.0.1.0");
                tw.WriteLine("==============================");
                foreach (var globalResource in TreeMap)
                {
                    await tw.WriteLineAsync("Global File Path: " + globalResource.Key);
                    await tw.WriteLineAsync("Source: ");
                    await tw.WriteLineAsync("\t FullPath: " + globalResource.Value.FullPath);
                    await tw.WriteLineAsync("\t Path: " + globalResource.Value.Path);
                    await tw.WriteLineAsync("\t Root: " + globalResource.Value.Root);
                    await tw.WriteLineAsync("\t Type: " + globalResource.Value.ResourceType?.Name ?? "EmptyType");
                    await tw.WriteLineAsync("\t Childrens: ");
                    foreach (var child in globalResource.Value.Childrens)
                    {
                        await DumpResource(child, "\t");
                    }
                }

                async Task DumpResource(IResourceInfo childResource, string line)
                {
                    await tw.WriteLineAsync(line + "\t FullPath: " + childResource.FullPath);
                    await tw.WriteLineAsync(line + "\t Path: " + childResource.Path);
                    await tw.WriteLineAsync(line + "\t Root: " + childResource.Root);
                    await tw.WriteLineAsync(line + "\t Type: " + childResource.ResourceType?.Name ?? "EmptyType");
                    await tw.WriteLineAsync(line + "\t Childrens: ");
                    foreach (var resource in childResource.Childrens)
                    {
                        await DumpResource(resource, line + "\t");
                    }
                }


                // Dump Atlases
                if (GameData.DarkWorldDLC) 
                {
                    DumpAtlas("BossAtlas", TFGame.BossAtlas, "DarkWorldContent/Atlas/bossAtlas.png");
                    DumpAtlas("Atlas", TFGame.Atlas, "DarkWorldContent/Atlas/atlas.png");
                }
                else 
                {
                    DumpAtlas("Atlas", TFGame.Atlas, "Content/Atlas/atlas.png");
                }

                DumpAtlas("MenuAtlas", TFGame.MenuAtlas, "Content/Atlas/menuAtlas.png");
                DumpAtlas("BGAtlas", TFGame.BGAtlas, "Content/Atlas/bgAtlas.png");

                }
            catch (Exception e)
            {
                Logger.Error("[DUMPRESOURCE]" + e.ToString());
                throw;
            }
        }

        private static void DumpAtlas(string name, Atlas atlas, string vanillaAtlas)
        {
            var injectedAtlases = new HashSet<string>(atlas.GetAllInjectedAtlas());
            injectedAtlases.Add(vanillaAtlas);
            foreach (var path in injectedAtlases) 
            {
                var pngPath = path;
                var xmlPath = path.Replace(".png", ".xml");

                using var pngStream = ModIO.OpenRead(pngPath);

                string pathName = name;
                if (path.StartsWith("mod:")) 
                {
                    pathName = Path.Combine(name, path.Substring(4, path.IndexOf('/') - 4));
                }

                if (!ModIO.IsFileExists(xmlPath)) 
                {
                    using CPUImage img = new CPUImage(pngStream);
                    int indexOfSlash = pngPath.IndexOf('/');
                    var keyPng = pngPath.Substring(indexOfSlash + 1).Replace("Content/Atlas/atlas/", "");
                    var dumpPath = $"DUMP/{pathName}/{keyPng}";
                    if (!Directory.Exists(Path.GetDirectoryName(dumpPath))) 
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));
                    }
                    img.SavePNG(dumpPath, img.Width, img.Height);
                    continue;
                }
                var atlasXml = ModIO.LoadXml(xmlPath);

                using CPUImage image = new CPUImage(pngStream);
                var subTextures = atlasXml["TextureAtlas"].GetElementsByTagName("SubTexture");
                foreach (XmlElement subTexture in subTextures) 
                {
                    var attrib = subTexture.Attributes;
                    string key = attrib["name"].Value;
                    var dumpPath = $"DUMP/{pathName}/{key}.png";

                    if (!Directory.Exists(Path.GetDirectoryName(dumpPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));

                    var x = Convert.ToInt32(attrib["x"].Value);
                    var y = Convert.ToInt32(attrib["y"].Value);
                    var width = Convert.ToInt32(attrib["width"].Value);
                    var height = Convert.ToInt32(attrib["height"].Value);
                    using var newImage = image.GetRegion(x, y, width, height);
                    newImage.SavePNG(dumpPath, width, height);
                }
            }
        }
    }

    public static class ResourceReloader 
    {
        public static Queue<FortContent> ContentRequestedReload = new Queue<FortContent>();

        public static void Update() 
        {
            if (ContentRequestedReload.Count == 0) { return; }

            var content = ContentRequestedReload.Dequeue();
            content?.Reload();
        }
    }
}
