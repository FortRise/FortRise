using System;
using System.IO;

namespace FortRise;

public class FolderModResource : ModResource
{
    public string FolderDirectory;
    public FolderModResource(ModuleMetadata metadata) : base(metadata)
    {
        FolderDirectory = metadata.PathDirectory.Replace('\\', '/');
    }

    public override void Lookup(string prefix)
    {
        var files = Directory.GetFiles(FolderDirectory);
        Array.Sort(files);
        for (int i = 0; i < files.Length; i++)
        {
            var filePath = files[i].Replace('\\', '/');

            var simplifiedPath = filePath.Replace(FolderDirectory + '/', "");
            var fileResource = new FileResourceInfo(this, simplifiedPath, filePath);
            Add(simplifiedPath, fileResource);
        }
        var folders = Directory.GetDirectories(FolderDirectory);
        Array.Sort(folders);
        foreach (var folder in folders)
        {
            var fixedFolder = folder.Replace('\\', '/');
            var simpliPath = fixedFolder.Replace(FolderDirectory + '/', "");

            var newFolderResource = new FileResourceInfo(this, simpliPath, fixedFolder);
            Lookup(prefix, folder, FolderDirectory, newFolderResource);
            Add(simpliPath, newFolderResource);
        }
    }

    public void Lookup(string prefix, string path, string modDirectory, FileResourceInfo folderResource)
    {
        var files = Directory.GetFiles(path);
        Array.Sort(files);
        for (int i = 0; i < files.Length; i++)
        {
            var filePath = files[i].Replace('\\', '/');

            var simplifiedPath = filePath.Replace(modDirectory + '/', "");
            var fileResource = new FileResourceInfo(this, simplifiedPath, filePath);
            Add(simplifiedPath, fileResource);
            folderResource.Childrens.Add(fileResource);
        }
        var folders = Directory.GetDirectories(path);
        Array.Sort(folders);
        foreach (var folder in folders)
        {
            var fixedFolder = folder.Replace('\\', '/');
            var simpliPath = fixedFolder.Replace(modDirectory + '/', "");

            var newFolderResource = new FileResourceInfo(this, simpliPath, prefix + simpliPath);
            Lookup(prefix, folder, modDirectory, newFolderResource);
            Add(simpliPath, newFolderResource);
            folderResource.Childrens.Add(newFolderResource);
        }
    }
}
