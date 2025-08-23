namespace FortRise;

internal sealed class ModEnvironment : IModEnvironment
{
    /// <summary>
    /// Indicates the current client's FortRise version.
    /// </summary>
    public SemanticVersion FortRiseVersion { get; init; }

    /// <summary>
    /// Indicates the current client's FortRise version.
    /// </summary>
    public string FortRisePath { get; init; }

    /// <summary>
    /// Indicates the current client's FortRise version.
    /// </summary>
    public string TowerFallPath { get; init; }

    public ModEnvironment(SemanticVersion fortRiseVersion, string fortRisePath, string towerFallPath) 
    {
        FortRiseVersion = fortRiseVersion;
        FortRisePath = fortRisePath;
        TowerFallPath = towerFallPath;
    }
}


