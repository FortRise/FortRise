using System.IO;
using TeuJson;

namespace FortRise;

/// <summary>
/// A FortRise module SaveData API allows to persists a data by saving and loading to a file.
/// Works via a Formatter built with <see href="https://www.nuget.org/packages/TeuJson">TeuJson</see>. 
/// </summary>
public abstract class ModuleSaveData 
{
    /// <summary>
    /// A formatter to use for saving and loading the data. Most notably Formatter is <see cref="JsonSaveDataFormat"/>.
    /// <br/>
    /// You can also implement your own Formatter.
    /// </summary>
    public SaveDataFormat Formatter { get; internal set; }
    
    /// <summary>
    /// A constructor that accepts an argument to create a Formatter.
    /// </summary>
    /// <param name="format">A formatter to use to be able to save and load your data. Most notably Formatter is <see cref="JsonSaveDataFormat"/></param>
    public ModuleSaveData(SaveDataFormat format) 
    {
        Formatter = format;
    }

    /// <summary>
    /// A function that is used to setup your saves and submit by closing a Formatter with a given input of your object.
    /// </summary>
    /// <param name="fortModule">A mod module</param>
    /// <returns>A closed Formatter to submit</returns>
    public abstract ClosedFormat Save(FortModule fortModule);

    /// <summary>
    /// A function that is used to assign your fields by using the Formatter. 
    /// The formatter is needed to be casted into a correct type of your formatter you used.
    /// <code>
    /// var castedFormatter = formatter.CastTo[T]();
    /// // where T : SaveDataFormat
    /// </code>
    /// </summary>
    /// <param name="formatter">A formatter you used that needed to be casted</param>
    public abstract void Load(SaveDataFormat formatter);

    /// <summary>
    /// A function that called when SaveData is being verified.
    /// </summary>
    public virtual void Verify() {}
}

/// <summary>
/// A finalized format to submit for save.
/// </summary>
public struct ClosedFormat
{
    internal SaveDataFormat Format;

    /// <summary>
    /// A constructor that created ClosedFormat from the Formatter.
    /// </summary>
    /// <param name="format">A formatter to be closed</param>
    public ClosedFormat(SaveDataFormat format) 
    {
        Format = format;
    }
}

/// <summary>
/// An abstract class for Formatter that is used for SaveData API.
/// </summary>
public abstract class SaveDataFormat 
{
    /// <summary>
    /// A path to save the path. This is set by the mod loader, but you used this in your own formatter.
    /// </summary>
    protected string SavePath;

    /// <summary>
    /// A file extension that this format used. (e.g json, xml, yaml)
    /// <br/>
    /// NOTE: Do not add '.' when filling up this property.
    /// </summary>
    public abstract string FileExtension { get; }

    internal void SetPath(FortModule module) 
    {
        SavePath = Path.Combine("Saves", module.ID, $"{module.Name}.saveData.{FileExtension}");
        if (!Directory.Exists(SavePath))    
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
    }

    /// <summary>
    /// A function that to save the object from a closed formatter to a file.
    /// </summary>
    public abstract void Save();

    /// <summary>
    /// A function to load an object and to store it inside of the formatter. 
    /// </summary>
    /// <returns>A success or failure boolean</returns>
    public abstract bool Load();

    /// <summary>
    /// A function to close a formatter without an object.
    /// </summary>
    /// <returns>A closed formatter</returns>
    public abstract ClosedFormat Close();

    /// <summary>
    /// A function to close a formatter.
    /// </summary>
    /// <param name="obj">An object to pass to the formatter to be stored inside</param>
    /// <returns>A closed formatter</returns>
    public abstract ClosedFormat Close(object obj);

}

/// <summary>
/// A utility and extension class for SaveDataFormat
/// </summary>
public static class SaveDataFormatExt 
{
    /// <summary>
    /// An extension that cast a Formatter to a correct Formatter.
    /// </summary>
    /// <param name="format">A formatter to be casted</param>
    /// <typeparam name="T">A SaveDataFormat type to used</typeparam>
    /// <returns>A Formatter with the corrected type</returns>
    public static T CastTo<T>(this SaveDataFormat format) 
    where T : SaveDataFormat
    {
        return format as T;
    }
}

/// <summary>
/// A built-in Json Binary formatter that can be use for constructing a SaveData module.
/// </summary>
public class JsonBinarySaveDataFormat : JsonSaveDataFormat
{
    /// <summary>
    /// A Json Binary file extension
    /// </summary>
    public override string FileExtension => "bin";

    /// <summary>
    /// Loads the SaveData Json Binary file. Handles automatically by the Mod Loader.
    /// </summary>
    /// <returns>A success or failure boolean</returns>
    public override bool Load()
    {
        if (!File.Exists(SavePath))
            return false;
        BaseValue = JsonBinaryReader.FromFile(SavePath).AsJsonObject;
        return true;
    }

    /// <summary>
    /// Save the SaveData Json Binary file. Handles automatically by the Mod Loader.
    /// </summary>
    public override void Save()
    {
        JsonBinaryWriter.WriteToFile(SavePath, BaseValue);
    }
}

/// <summary>
/// A built-in Json formatter that can be use for constructing a SaveData module.
/// </summary>
public class JsonSaveDataFormat : SaveDataFormat
{
    /// <summary>
    /// A stored base value that handles all of the necessary data for your saves.
    /// </summary>
    protected JsonValue BaseValue;
    /// <summary>
    /// A Json file extension
    /// </summary>
    public override string FileExtension => "json";

    /// <summary>
    /// A constructor that create the formatter.
    /// </summary>
    /// <returns></returns>
    public JsonSaveDataFormat() 
    {
    }

    /// <summary>
    /// Loads the SaveData Json file. Handles automatically by the Mod Loader.
    /// </summary>
    /// <returns>A success or failure boolean</returns>
    public override bool Load()
    {
        if (!File.Exists(SavePath))
            return false;
        BaseValue = JsonTextReader.FromFile(SavePath);
        return true;
    }

    /// <summary>
    /// Save the SaveData Json file. Handles automatically by the Mod Loader.
    /// </summary>
    public override void Save()
    {
        JsonTextWriter.WriteToFile(SavePath, BaseValue);
    }

    /// <summary>
    /// A function that get the Json Object with the necessary data for your saves.
    /// </summary>
    /// <returns>A Json Object</returns>
    public JsonValue GetJsonObject() => BaseValue;

    /// <summary>
    /// A function to close a formatter and stores Json Object.
    /// </summary>
    /// <param name="obj">A Json Object to pass to the formatter to be stored inside</param>
    /// <returns>A closed formatter</returns>
    public ClosedFormat Close(JsonObject obj) 
    {
        BaseValue = obj;
        return new ClosedFormat(this);
    }

    /// <summary>
    /// A function to close a formatter. (Recommend to use the Close(JsonObject obj) overload instead)
    /// </summary>
    /// <param name="obj">An object to pass to the formatter to be stored inside</param>
    /// <returns>A closed formatter</returns>
    public override ClosedFormat Close(object obj)
    {
        if (obj is not JsonValue)
        {
            Logger.Error("Close object must be of type JsonValue.");
            return new ClosedFormat(this);
        }
        BaseValue = (JsonValue)obj;
        return Close();
    }

    /// <summary>
    /// A function to close a formatter without an object.
    /// </summary>
    /// <returns>A closed formatter</returns>
    public override ClosedFormat Close()
    {
        return new ClosedFormat(this);
    }
}