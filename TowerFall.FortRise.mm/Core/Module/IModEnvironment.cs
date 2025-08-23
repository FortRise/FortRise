namespace FortRise;

public interface IModEnvironment 
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
}

