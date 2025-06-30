#nullable enable
using System.Collections.Generic;

namespace FortRise;

public interface IModInterop
{
    IReadOnlyList<IModResource> LoadedMods { get; }
    IReadOnlyList<Mod> LoadedFortModules {get; }

    string[]? GetTags(string modName);
    string[] GetAllTags();
    IModResource? GetMod(string tag);
    IReadOnlyList<IModResource> GetModDependents();
    IReadOnlyList<IModResource> GetModsByTag(string tag);
    IModRegistry? GetModRegistry(string modName);
    IModRegistry? GetModRegistry(ModuleMetadata metadata);
    bool IsModDepends(ModuleMetadata metadata);

    T? GetApi<T>(string name, Option<SemanticVersion> minimumVersion = default) where T : class;

    bool IsModExists(string name);
}
