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

    public virtual void Verify() {}
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
        if (!Directory.Exists(SavePath))    
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
    }

    public abstract void Save();
    public abstract bool Load();
    public abstract ClosedFormat Close();
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

    public override bool Load()
    {
        if (!File.Exists(SavePath))
            return false;
        BaseValue = JsonBinaryReader.FromFile(SavePath).AsJsonObject;
        return true;
    }

    public override void Save()
    {
        JsonBinaryWriter.WriteToFile(SavePath, BaseValue);
    }
}

public class JsonSaveDataFormat : SaveDataFormat
{
    protected JsonValue BaseValue;

    public override string FileExtension => "json";

    public JsonSaveDataFormat() : base()
    {
    }

    public override bool Load()
    {
        if (!File.Exists(SavePath))
            return false;
        BaseValue = JsonTextReader.FromFile(SavePath);
        return true;
    }

    public override void Save()
    {
        JsonTextWriter.WriteToFile(SavePath, BaseValue);
    }

    public JsonValue GetJsonObject() => BaseValue;

    public ClosedFormat Close(JsonObject obj) 
    {
        BaseValue = obj;
        return new ClosedFormat(this);
    }

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

    public override ClosedFormat Close()
    {
        return new ClosedFormat(this);
    }
}