using System;
using System.Collections.Generic;
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
    public string MetaName { get; internal set; }
    public Version MetaVersion { get; internal set; }
    public string MetaDescription { get; internal set; }
    public string MetaAuthor { get; internal set; }
    public virtual Type SettingsType { get; }
    public ModuleSettings InternalSettings;
    public abstract void Load();
    public abstract void Unload();

    public void InternalLoad() 
    {
        LoadSettings();
        Load();
    }

    public void LoadSettings() 
    {
        InternalSettings = (ModuleSettings)SettingsType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
    }

    public void CreateSettings(List<OptionsButton> optionsList) 
    {
        var type = SettingsType;
        var settings = InternalSettings;


        if (settings == null || type == null) 
            return;

        
        foreach (var field in type.GetFields()) 
        {
            if (field.IsPrivate)
                continue;
            var name = field.Name;
            var fieldType = field.FieldType;
            SettingsNumberAttribute attrib = null;

            var ownName = field.GetCustomAttribute<SettingsNameAttribute>();
            if (ownName != null)
                name = ownName.Name;

            var fullName = $"{Name} {name}".ToUpper();

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