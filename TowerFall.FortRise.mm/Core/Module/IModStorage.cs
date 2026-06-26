#nullable enable
namespace FortRise;

public interface IModStorage
{
    public string StoragePath { get; }

    IStorageResourceInfo? Open(string filepath);
    IStorageResourceInfo? OpenOrCreate(string filepath);
    IStorageResourceInfo CreateDirectory(string filepath);
    bool Delete(string filepath, bool recursive);
    bool Exists(string filepath);
    void WriteAllText(string filepath, string text);
}
