using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TowerFall;

namespace FortRise;

public class FortAttribute : Attribute 
{
    public string GUID;
    public string Name;

    public FortAttribute(string guid, string name) 
    {
        GUID = guid;
        Name = name;
    }
}

public abstract partial class FortModule 
{
    public bool Enabled { get; internal set; }
    public string Name { get; internal set; }
    public string ID { get; internal set; }
    public ModuleMetadata Meta { get; internal set; }
    public bool SupportModDisabling { get; set; } = true;
    public bool RequiredRestart { get; set; }

    public virtual Type SettingsType { get; }
    public ModuleSettings InternalSettings;
    public virtual Type SaveDataType { get; }
    public ModuleSaveData InternalSaveData;
    public FortContent Content;


    public abstract void Load();
    public abstract void Unload();


    internal void InternalLoad() 
    {
        LoadSettings();
        Load();
    }

    internal void InternalUnload() 
    {
        Content?.Unload();
        Unload();
    }

    internal void SaveData() 
    {
        if (InternalSaveData == null)
            return;

        var format = InternalSaveData.Save(this).Format;
        format.SetPath(this);
        format.Save();
    }

    internal void VerifyData() 
    {
        if (InternalSaveData == null)
            return;

        InternalSaveData.Verify();
    }

    internal void LoadData() 
    {
        InternalSaveData = (ModuleSaveData)SaveDataType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        if (InternalSaveData == null)
            return;

        var format = InternalSaveData.Formatter;
        format.SetPath(this);
        if (format.Load())
            InternalSaveData.Load(format);
    }

    public void LoadSettings() 
    {
        InternalSettings = (ModuleSettings)SettingsType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());

        if (InternalSettings == null)
            return;

        var path = Path.Combine("Saves", ID, Name + ".settings" + ".json");
        InternalSettings.Load(path);
    }

    public void SaveSettings() 
    {
        if (InternalSettings == null)
            return;
        var path = Path.Combine("Saves", ID, Name + ".settings" + ".json");
        InternalSettings.Save(path);
    }

    public virtual void CreateModSettings(List<OptionsButton> optionList) {}

    internal void CreateSettings(List<OptionsButton> optionsList) 
    {
        CreateModSettings(optionsList);

        var type = SettingsType;
        var settings = InternalSettings;

        if (settings == null || type == null) 
            return;

        // where the automated settings are created
        foreach (var field in type.GetFields()) 
        {
            if (field.IsPrivate)
                continue;
            var name = field.Name;
            var fieldType = field.FieldType;
            SettingsNumberAttribute attrib = null;
            SettingsOptionsAttribute optAttrib = null;

            var ownName = field.GetCustomAttribute<SettingsNameAttribute>();
            if (ownName != null)
                name = ownName.Name;

            var fullName = $"{name}".ToUpperInvariant();

            if (fieldType == typeof(bool)) 
            {
                var optionButton = new OptionsButton(fullName);
                optionButton.SetCallbacks(() => {
                    optionButton.State = BoolToString((bool)field.GetValue(settings));
                }, null, null, () => {
                    var val = !(bool)field.GetValue(settings);
                    field.SetValue(settings, val);
                    return val;
                });
                optionsList.Add(optionButton);
            }
            else if (fieldType == typeof(Action)) 
            {
                var optionButton = new OptionsButton(fullName);
                optionButton.SetCallbacks(() => {
                    var action = (Action)field.GetValue(settings);
                    action();
                });
                optionsList.Add(optionButton);
            }
            else if ((fieldType == typeof(int)) && (optAttrib = field.GetCustomAttribute<SettingsOptionsAttribute>()) != null) 
            {
                var optionButton = new OptionsButton(fullName);
                optionButton.SetCallbacks(() => {
                    var value = (int)field.GetValue(settings);
                    optionButton.State = optAttrib.Options[value].ToUpperInvariant();
                    optionButton.CanLeft = (value > 0);
                    optionButton.CanRight = (value < optAttrib.Options.Length - 1);
                }, () => {
                    var value = (int)field.GetValue(settings);
                    value -= 1;
                    field.SetValue(settings, value);
                }, () => {
                    var value = (int)field.GetValue(settings);
                    value += 1;
                    field.SetValue(settings, value);
                }, null);
                optionsList.Add(optionButton);
            }
            else if ((fieldType == typeof(string)) && (optAttrib = field.GetCustomAttribute<SettingsOptionsAttribute>()) != null) 
            {
                var optionButton = new OptionsButton(fullName);
                optionButton.SetCallbacks(() => {
                    var value = (string)field.GetValue(settings);
                    var index = Array.IndexOf(optAttrib.Options, value);
                    optionButton.State = optAttrib.Options[index].ToUpperInvariant();
                    optionButton.CanLeft = (index > 0);
                    optionButton.CanRight = (index < optAttrib.Options.Length - 1);
                }, () => {
                    var value = (string)field.GetValue(settings);
                    var index = Array.IndexOf(optAttrib.Options, value);
                    index -= 1;
                    value = optAttrib.Options[index];
                    field.SetValue(settings, value);
                }, () => {
                    var value = (string)field.GetValue(settings);
                    var index = Array.IndexOf(optAttrib.Options, value);
                    index += 1;
                    value = optAttrib.Options[index];
                    field.SetValue(settings, value);
                }, null);
                optionsList.Add(optionButton);
            }
            else if ((fieldType == typeof(int) || fieldType == typeof(float)) && 
                (attrib = field.GetCustomAttribute<SettingsNumberAttribute>()) != null) 
            {
                var optionButton = new OptionsButton(fullName);
                optionButton.SetCallbacks(() => {
                    if (fieldType == typeof(float)) 
                    {
                        var value = (float)field.GetValue(settings);
                        optionButton.State = value.ToString();
                        optionButton.CanLeft = (value > attrib.Min);
                        optionButton.CanRight = (value < attrib.Max);
                    }
                    else 
                    {
                        var value = (int)field.GetValue(settings);
                        optionButton.State = value.ToString();
                        optionButton.CanLeft = (value > attrib.Min);
                        optionButton.CanRight = (value < attrib.Max);
                    }

                }, () => {
                    if (fieldType == typeof(float)) 
                    {
                        var value = (float)(field.GetValue(settings));
                        value -= attrib.Step;
                        field.SetValue(settings, value);
                    }
                    else 
                    {
                        var value = (int)(field.GetValue(settings));
                        value -= attrib.Step;
                        field.SetValue(settings, value);
                    }
                }, () => {
                    if (fieldType == typeof(float)) 
                    {
                        var value = (float)(field.GetValue(settings));
                        value += attrib.Step;
                        field.SetValue(settings, value);
                    }
                    else 
                    {
                        var value = (int)(field.GetValue(settings));
                        value += attrib.Step;
                        field.SetValue(settings, value);
                    }
                }, null);
                optionsList.Add(optionButton);
            }
        }
        

        
        string BoolToString(bool value)
        {
            if (!value)
            {
                return "OFF";
            }
            return "ON";
        }
    }


    public virtual void LoadContent() {}
    public virtual void Initialize() {}
    public virtual void OnVariantsRegister(MatchVariants variants, bool noPerPlayer = false) {}
}

public class ModuleMetadata : IEquatable<ModuleMetadata>
{
    public string Name;
    public Version Version;
    public Version FortRiseVersion;
    public string Description;
    public string Author;
    public string DLL;
    public string PathDirectory;
    public string PathZip;
    public string[] Dependencies;
    public string NativePath;
    public string NativePathX86;

    internal ModuleMetadata() {}


    public override string ToString()
    {
        return $"Metadata: {Name} by {Author} {Version}";
    }


    public bool Equals(ModuleMetadata other)
    {
        if (other.Name != this.Name)
            return false;
        
        if (other.Version != this.Version)
            return false;
        
        if (!string.IsNullOrEmpty(other.Author) && other.Author != this.Author)
            return false;

        return true;
    }

    public override bool Equals(object obj) => Equals(obj as ModuleMetadata);
    

    public override int GetHashCode()
    {
        var version = Version.GetHashCode();
        var name = Name.GetHashCode();
        var author = Author.GetHashCode();
        return version + name + author;
    }

    public static bool operator ==(ModuleMetadata lhs, ModuleMetadata rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ModuleMetadata lhs, ModuleMetadata rhs) => !(lhs == rhs);
}