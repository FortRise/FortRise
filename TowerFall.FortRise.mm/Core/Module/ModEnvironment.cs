namespace FortRise;

internal sealed class ModEnvironment : IModEnvironment
{
    public SemanticVersion FortRiseVersion { get; init; }

    public ModEnvironment(SemanticVersion fortRiseVersion) 
    {
        FortRiseVersion = fortRiseVersion;
    }
}


