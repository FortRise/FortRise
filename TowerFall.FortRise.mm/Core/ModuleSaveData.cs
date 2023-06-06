using System.IO;
using TeuJson;

namespace FortRise;

public abstract class ModuleSaveData 
{
    public SaveDataFormat Formatter { get; internal set; }
    public ModuleSaveData(SaveDataFormat format) 
    {
        Formatter = format;
    }

    public abstract ClosedFormat Save(FortModule fortModule);
    public abstract void Load(SaveDataFormat formatter);
}


public struct ClosedFormat
{
    internal SaveDataFormat Format;

    public ClosedFormat(SaveDataFormat format) 
    {
        Format = format;
    }
}


public abstract class SaveDataFormat 
{
    protected string SavePath;

    public abstract string FileExtension { get; }

    internal void SetPath(FortModule module) 
    {
        SavePath = Path.Combine("Saves", module.ID, $"{module.Name}.saveData.{FileExtension}");
    }

    public abstract void Save();
    public abstract void Load();
    public abstract ClosedFormat Close(object obj);

}

public static class SaveDataFormatExt 
{
    public static T CastTo<T>(this SaveDataFormat format) 
    where T : SaveDataFormat
    {
        return format as T;
    }
}

public class JsonBinarySaveDataFormat : JsonSaveDataFormat
{
    public override string FileExtension => "bin";

    public override void Load()
    {
        if (!File.Exists(SavePath))
            return;
        BaseObject = JsonBinaryReader.FromFile(SavePath).AsJsonObject;
    }

    public override void Save()
    {
        JsonBinaryWriter.WriteToFile(SavePath, BaseObject);
    }
}

public class JsonSaveDataFormat : SaveDataFormat
{
    protected JsonObject BaseObject;

    public override string FileExtension => "json";

    public JsonSaveDataFormat() : base()
    {
    }

    public override void Load()
    {
        if (!File.Exists(SavePath))
            return;
        BaseObject = JsonTextReader.FromFile(SavePath).AsJsonObject;
    }

    public override void Save()
    {
        JsonTextWriter.WriteToFile(SavePath, BaseObject);
    }

    public JsonValue GetJsonObject() => BaseObject;

    public ClosedFormat Close(JsonObject obj) 
    {
        BaseObject = obj;
        return new ClosedFormat(this);
    }

    public override ClosedFormat Close(object obj)
    {
        if (obj is not JsonObject)
        {
            Logger.Error("Close object must be of type JsonObject.");
            return new ClosedFormat(this);
        }
        BaseObject = (JsonObject)obj;
        return new ClosedFormat(this);
    }
}