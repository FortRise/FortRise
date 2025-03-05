using MonoMod;

namespace TowerFall;

public static class patch_GifExportOptions 
{
    [PatchSDL2ToSDL3]
    [MonoModIgnore]
    private static extern string GetDocumentsFolder();
}