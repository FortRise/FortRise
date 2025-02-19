using System.IO;

namespace FortRise;

/// <summary>
/// A FortRise module SaveData API allows to persists a data by saving and loading to a file.
/// </summary>
public abstract class ModuleSaveData 
{
    /// <summary>
    /// A function that called when SaveData is being verified.
    /// </summary>
    public virtual void Verify() {}
}