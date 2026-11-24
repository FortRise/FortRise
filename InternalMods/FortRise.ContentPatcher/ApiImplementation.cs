namespace FortRise.ContentPatcher;

internal sealed class ApiImplementation(SubtexturePatcher subtexturePatcher, LevelPatcher levelPatcher) : IFortRiseContentPatcherAPI
{
    public IFortRiseContentPatcherAPI.IPatchHandle RegisterLevelPatch(ModuleMetadata owner, IFortRiseContentPatcherAPI.ILevelPatch patch)
    {
        return levelPatcher.Register(owner, patch);
    }

    public IFortRiseContentPatcherAPI.IPatchHandle RegisterSubtexturePatch(ModuleMetadata owner, IFortRiseContentPatcherAPI.ISubtexturePatch patch)
    {
        return subtexturePatcher.Register(owner, patch);
    }

    public IFortRiseContentPatcherAPI.ILevelInfo? GetCurrentLevel() => levelPatcher.GetCurrentLevel();

    public void UnregisterAll(ModuleMetadata owner)
    {
        subtexturePatcher.RemoveAll(owner);
        levelPatcher.RemoveAll(owner);
    }
}
