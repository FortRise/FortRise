using System;
using System.IO;
using System.Linq;
using System.Xml;
using Monocle;

namespace FortRise.IO;

public static class ModFs 
{
    public static bool IsFolder(RiseCore.Resource resource) 
    {
        return resource.ResourceType == typeof(RiseCore.ResourceTypeFolder);
    }

    public static bool IsFile(RiseCore.Resource resource) 
    {
        return resource.ResourceType != typeof(RiseCore.ResourceTypeFolder);
    }

    public static bool IsFolder(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res)) 
        {
            return IsFolder(res);
        }
        FileAttributes attr = File.GetAttributes(path);
        return attr == FileAttributes.Directory;
    }

    public static bool IsFile(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res)) 
        {
            return IsFile(res);
        }
        FileAttributes attr = File.GetAttributes(path);
        return attr != FileAttributes.Directory;
    }

    public static bool IsDirectoryOrFileExists(string path) 
    {
        return RiseCore.ResourceTree.IsExist(path) || Directory.Exists(path) || File.Exists(path);
    }

    public static XmlDocument LoadXml(RiseCore.Resource resource) 
    {
        var text = ModFs.OpenRead(resource);
        return patch_Calc.LoadXML(text);
    }

    public static XmlDocument LoadXml(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var resource)) 
        {
            return ModFs.LoadXml(resource);
        }
        return Calc.LoadXML(path);
    }

    public static string[] GetFiles(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res)) 
        {
            // FIXME: performance improvemnts
            var childs = res.Childrens.Where(x => x.ResourceType != typeof(RiseCore.ResourceTypeFolder)).ToList();
            string[] folders = new string[childs.Count];
            for (int i = 0; i < childs.Count; i++) 
            {
                var child = childs[i];
                folders[i] = child.RootPath;
            }

            return folders;
        }
        return Directory.GetFiles(path);
    }

    public static string[] GetDirectories(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res)) 
        {
            // FIXME: performance improvemnts
            var childs = res.Childrens.Where(x => x.ResourceType == typeof(RiseCore.ResourceTypeFolder)).ToList();
            string[] folders = new string[childs.Count];
            for (int i = 0; i < childs.Count; i++) 
            {
                var child = childs[i];
                folders[i] = child.RootPath;
            }

            return folders;
        }
        return Directory.GetDirectories(path);
    }

    public static Stream OpenRead(RiseCore.Resource resource) 
    {
        return resource.Stream;
    }

    public static Stream OpenRead(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            return OpenRead(res);
        }

        return File.OpenRead(path);
    }

    public static Stream Open(string path, FileMode mode) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            var rfs = res.Stream;
            return rfs;
        }

        return File.Open(path, mode);
    }

    public static StreamReader OpenText(RiseCore.Resource resource) 
    {
        using var fs = resource.Stream;
        return new StreamReader(fs);
    }

    public static StreamReader OpenText(string path) 
    {
        if (RiseCore.ResourceTree.TryGetValue(path, out var res))
        {
            using var rfs = res.Stream;
            StreamReader reader = new StreamReader(rfs);
            return reader;
        }

        return File.OpenText(path);
    }

    public static void LoopAllModsContent(Action<FortContent> contentCallback) 
    {
        RiseCore.ResourceTree.LoopThroughModsContent(contentCallback);
    }
}

public static class ExampleAPI 
{
    static FortContent Content;
    public static void FortRiseIO() 
    {
        using TextReader reader = ModFs.OpenText("Text.txt");
        string x = reader.ReadToEnd(); // returns a string

        using TextReader reader2 = ModFs.OpenText("mod:WiderSetMod/Content/Text.txt");
        string z = reader.ReadToEnd(); // returns a string

        // Mimicing how ArcherLoader loads its archer, but inside the Mods' Content instead.
        ModFs.LoopAllModsContent(x => {
            string path = Path.Combine(x.MetadataPath, "Content", "Archers");

            string[] archerDirectories = ModFs.GetDirectories(path);

            foreach (var archer in archerDirectories) 
            {
                var archerDataXmlPath = Path.Combine(archer, "archerData.xml");
                var archerDataXml = ModFs.LoadXml(archerDataXmlPath);


                // Some process.
            }
        });
    }

    public static void SystemIO() 
    {
        using TextReader reader = File.OpenText("Text.txt");
        string x = reader.ReadToEnd(); // returns a string
    }
}

