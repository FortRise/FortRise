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

        private static string Modize(ReadOnlySpan<char> path)
        {
            Span<char> dest = path.Length < 2048 ? stackalloc char[path.Length] : new char[path.Length];
            path.Replace(dest, '\\', '/');
            return new string(dest);
        }

        public static IResourceInfo Get(string path)
        {
            if (!TryGetValue(path, out var res))
            {
                throw new Exception($"Resource path: '{Modize(path)}' not found or does not exists.");
            }
            return res;
        }

        public static bool TryGetValue(string path, out IResourceInfo res)
        {
            var modizedPath = Modize(path);
            TreeMap.TryGetValue(modizedPath, out res);
            if (res == null)
            {
                if (modizedPath.EndsWith('/'))
                {
                    modizedPath = modizedPath[0..(path.Length - 1)];
                    TreeMap.TryGetValue(modizedPath, out res);
                    if (res != null)
                    {
                        return true;
                    }
                }
                else
                {
                    modizedPath += '/';
                    TreeMap.TryGetValue(modizedPath, out res);
                    if (res != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        public static bool IsExist(string path)
        {
            return TreeMap.ContainsKey(Modize(path));
        }

        public static bool IsExist(IResourceInfo resource, string path)
        {
            return TreeMap.ContainsKey((resource.Root + Modize(path)));
        }

        // If you were expecting this feature, please use the RiseCore.Events.OnAfterModdedLoadContent event instead.
        internal static void AfterModdedLoadContent()
        {
            foreach (var mod in ModResources)
            {
                if (mod.Content == null)
                    continue;
                // Events.Invoke_OnAfterModdedLoadContent(mod.Content);
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
}
