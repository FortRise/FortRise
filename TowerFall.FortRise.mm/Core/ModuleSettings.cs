using System;
using System.IO;
using System.Reflection;
using TeuJson;

namespace FortRise;

/// <summary>
/// A FortRise module settings API that automatically built a settings option for your module. 
/// </summary>
public abstract class ModuleSettings 
{
    /// <summary>
    /// Saves your settings path. This handles automatically by the mod loader.
    /// </summary>
    /// <param name="path">A path to save</param>
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
            else if (field.FieldType == typeof(string)) 
            {
                json[key] = (string)value;
            }
            else 
            {
                Logger.Error("[Settings] Type unsupported: " + field.FieldType);
            }
        }
        JsonTextWriter.WriteToFile(path, json);
    }

    /// <summary>
    /// Loads settings from a given path. This handles automatically by the mod loader.
    /// </summary>
    /// <param name="path">A path to load</param>
    public void Load(string path) 
    {
        if (!File.Exists(path))
            return;
        var thisType = this.GetType();
        var json = JsonTextReader.FromFile(path);
        foreach (var val in json.Pairs) 
        {
            var field = thisType.GetField(val.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
                continue;

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

/// <summary>
/// An attribute marker to change the visual name of a field inside of an option.
/// NOTE: This does not rename the field name when save.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingsNameAttribute : Attribute 
{
    /// <summary>
    /// A name of an option.
    /// </summary>
    public string Name;

    /// <summary>
    /// Marked a field to change the visual name of a field inside of an option.
    /// </summary>
    /// <param name="name">A name for the option</param>
    public SettingsNameAttribute(string name) 
    {
        Name = name;
    }
}

/// <summary>
/// An attribute marker to change an option to be selectable as number.
/// It can specify a minimum length, maximum length, and how it step through the number.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingsNumberAttribute : Attribute 
{
    /// <summary>
    /// A minimum length.
    /// </summary>
    public int Min;
    /// <summary>
    /// A maximum length.
    /// </summary>
    public int Max;
    /// <summary>
    /// A step on how it change through the number.
    /// </summary>
    public int Step;

    /// <summary>
    /// Marked a field of float or int to change an option to be selectable as number.
    /// </summary>
    /// <param name="min">A minimum length</param>
    /// <param name="max">A maximum length</param>
    /// <param name="step">A step on how it change through the number</param>
    public SettingsNumberAttribute(int min = 0, int max = 100, int step = 1) 
    {
        Min = min;
        Max = max;
        Step = step;
    }
}

/// <summary>
/// An attribute marker that turns an option to have an ability to select a different option.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingsOptionsAttribute : Attribute 
{
    /// <summary>
    /// An array of options. 
    /// </summary>
    public string[] Options;

    /// <summary>
    /// Marked a field of string to have a different option to select.
    /// </summary>
    /// <param name="options">An options to select inside of the settings</param>
    public SettingsOptionsAttribute(params string[] options) 
    {
        Options = options;
    }
}