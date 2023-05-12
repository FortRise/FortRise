using System;
using System.IO;
using System.Reflection;
using TeuJson;

namespace FortRise;

public abstract class ModuleSettings 
{
    public void Save(string path) 
    {
        if (!Directory.Exists(path))    
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        var json = new JsonObject();
        foreach (var field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) 
        {
            if (field.FieldType == typeof(Action))
                continue;
            var key = field.Name;
            var value = field.GetValue(this);
            if (field.FieldType == typeof(bool)) 
            {
                json[key] = (bool)value;
            }
            else if (field.FieldType == typeof(int)) 
            {
                json[key] = (int)value;
            }
            else if (field.FieldType == typeof(float)) 
            {
                json[key] = (float)value;
            }
        }
        JsonTextWriter.WriteToFile(path, json);
    }

    public void Load(string path) 
    {
        if (!File.Exists(path))
            return;
        var thisType = this.GetType();
        var json = JsonTextReader.FromFile(path);
        foreach (var val in json.Pairs) 
        {
            var field = thisType.GetField(val.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field.FieldType == typeof(bool)) 
            {
                field.SetValue(this, json[val.Key].AsBoolean);
            }
            else if (field.FieldType == typeof(int)) 
            {
                field.SetValue(this, json[val.Key].AsInt32);
            }
            else if (field.FieldType == typeof(float)) 
            {
                field.SetValue(this, json[val.Key].AsSingle);
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingsNameAttribute : Attribute 
{
    public string Name;

    public SettingsNameAttribute(string name) 
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingsNumberAttribute : Attribute 
{
    public int Min;
    public int Max;
    public int Step;

    public SettingsNumberAttribute(int min = 0, int max = 100, int step = 1) 
    {
        Min = min;
        Max = max;
        Step = step;
    }
}