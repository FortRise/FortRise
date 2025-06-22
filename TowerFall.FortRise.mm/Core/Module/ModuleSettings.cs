using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace FortRise;

public interface ISettingsCreate
{
    TextContainer Container { get; init; }

    void CreateOnOff(string name, bool initialValue, Action<bool> onPressed);
    void CreateOptions(string name, string initialValue, string[] selections, Action<(string, int)> onSelect);
    void CreateButton(string name, Action onPressed);
    void CreateNumber(string name, int initialValue, Action<int> onChanged, int min = 0, int max = 10, int step = 1);
    void CreateInput(string name, string initialValue, Action<string> onInput, TextContainer.InputText.InputBehavior inputBehavior = TextContainer.InputText.InputBehavior.None);
}

internal sealed class SettingsCreate : ISettingsCreate
{
    public TextContainer Container { get; init; }

    public SettingsCreate(TextContainer container)
    {
        Container = container;
    }

    public void CreateButton(string name, Action onPressed)
    {
        string fullName = name.ToUpperInvariant();
        var numberButton = new TextContainer.ButtonText(fullName);
        numberButton.OnConfirm = onPressed;

        Container.Add(numberButton);
    }

    public void CreateInput(string name, string initialValue, Action<string> onInput, TextContainer.InputText.InputBehavior inputBehavior = TextContainer.InputText.InputBehavior.None)
    {
        string fullName = name.ToUpperInvariant();
        var numberButton = new TextContainer.InputText(fullName, initialValue, inputBehavior);
        numberButton.OnInputEntered = onInput;

        Container.Add(numberButton);
    }

    public void CreateNumber(string name, int initialValue, Action<int> onChanged, int min = 0, int max = 10, int step = 1)
    {
        string fullName = name.ToUpperInvariant();
        var numberButton = new TextContainer.Number(fullName, initialValue, min, max, step);
        numberButton.Change(onChanged);

        Container.Add(numberButton);
    }

    public void CreateOnOff(string name, bool initialValue, Action<bool> onPressed)
    {
        string fullName = name.ToUpperInvariant();
        var numberButton = new TextContainer.Toggleable(fullName, initialValue);
        numberButton.Change(onPressed);

        Container.Add(numberButton);
    }

    public void CreateOptions(string name, string initialValue, string[] selections, Action<(string, int)> onSelect)
    {
        string fullName = name.ToUpperInvariant();
        var selectionOption = new TextContainer.SelectionOption(
            fullName, selections, Array.IndexOf(selections, initialValue));
        selectionOption.Change(onSelect);
        Container.Add(selectionOption);
    }
}

/// <summary>
/// A FortRise module settings API that automatically built a settings option for your module. 
/// </summary>
public abstract class ModuleSettings
{
    public abstract void Create(ISettingsCreate settings);

    /// <summary>
    /// Saves your settings path. This handles automatically by the mod loader.
    /// </summary>
    /// <param name="path">A path to save</param>
    public void Save(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        var json = new Dictionary<string, object>();
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
        var jsonText = JsonSerializer.Serialize(json);
        File.WriteAllText(path, jsonText);
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
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path));
        foreach (var val in json)
        {
            var field = thisType.GetField(val.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
                continue;

            if (field.FieldType == typeof(bool))
            {
                field.SetValue(this, json[val.Key].GetBoolean());
            }
            else if (field.FieldType == typeof(int))
            {
                field.SetValue(this, json[val.Key].GetInt32());
            }
            else if (field.FieldType == typeof(float))
            {
                field.SetValue(this, json[val.Key].GetSingle());
            }
            else if (field.FieldType == typeof(string))
            {
                field.SetValue(this, json[val.Key].GetString());
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

/// <summary>
/// An attribute marker that hides or ignore the creation of this settings. Useful for creating custom settings.
/// Note that the field will still be serialized.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingsHideAttribute : Attribute {}

/// <summary>
/// An attribute marker that makes the button shows a text input. Field should be marked as string when using this.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SettingsInputAttribute : Attribute 
{
    public TextContainer.InputText.InputBehavior InputBehavior { get; }

    public SettingsInputAttribute(TextContainer.InputText.InputBehavior inputBehavior)
    {
        InputBehavior = inputBehavior;
    }

    public SettingsInputAttribute()
    {
        InputBehavior = TextContainer.InputText.InputBehavior.None;
    }
}