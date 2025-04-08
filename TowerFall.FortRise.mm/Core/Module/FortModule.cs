using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FortRise.IO;
using MonoMod;

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

[MonoModRemove]
public abstract partial class patch_FortModule
{
    public FortContent Content 
    {
        get;
        internal set;
    }
}

public abstract partial class FortModule
{
    public bool Enabled { get; internal set; }
    public ModuleMetadata Meta { get; internal set; }
    public string Name { get; internal set; }
    public string ID { get; internal set; }
    public bool DisposeTextureAfterUnload { get; set; } = true;
    public bool IsInitialized { get; internal set; }

    public IModRegistry Registry 
    {
        get 
        {
            if (!IsInitialized)
            {
                throw new Exception("Mod is not initialized yet!");
            }

            return registry;
        }
        internal set => registry = value;
    }

    private IModRegistry registry;

    /// <summary>
    /// Use to let the mod loader know which settings type to initialize.
    /// </summary>
    public virtual Type SettingsType { get; }
    /// <summary>
    /// An initialized settings from <see cref="FortModule.SettingsType"/>. Cast this with your own settings type.
    /// </summary>
    public ModuleSettings InternalSettings;
    public virtual Type SaveDataType { get; }
    public ModuleSaveData InternalSaveData;

    /// <summary>
    /// The module's mod content which use to load atlases, spriteDatas, SFXes, etc..
    /// </summary>
    [MonoModLinkTo("FortRise.FortModule", "get_Content")]
    [MonoModRemove]
    public FortContent Content;

    [MonoModLinkTo("FortRise.FortModule", "set_Content")]
    [MonoModRemove]
    public void s_Content(FortContent content) {}

    /// <summary>
    /// Override this function to load your hooks, events, and set environment variables for your mod.
    /// <br/>
    /// DO NOT LOAD YOUR CONTENTS HERE OR INITIALIZE SOMETHING.
    /// <br/>
    /// Use <see cref="FortModule.LoadContent"/>
    /// or <see cref="FortModule.Initialize"/> instead.
    /// </summary>
    public abstract void Load();

    /// <summary>
    /// Override this function to unload your hooks or dispose your resources.
    /// </summary>
    public abstract void Unload();

    internal void InternalLoad()
    {
        LoadSettings();
        Load();
    }

    internal void InternalUnload()
    {
        Content?.Unload(DisposeTextureAfterUnload);
        Unload();
    }

    internal void SaveData()
    {
        if (InternalSaveData == null)
        {
            return;
        }

        try 
        {
            var savePath = Path.Combine(GetRootPath(), "Saves", ID, $"{Name}.saveData.json");
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            var json = JsonSerializer.Serialize(InternalSaveData, SaveDataType);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }

    internal void VerifyData()
    {
        if (InternalSaveData == null)
            return;

        InternalSaveData.Verify();
    }

    internal void LoadData()
    {
        if (SaveDataType == null)
        {
            return;
        }
        var savePath = Path.Combine(GetRootPath(), "Saves", ID, $"{Name}.saveData.json");
        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        }

        if (File.Exists(savePath))
        {
            InternalSaveData = (ModuleSaveData)JsonSerializer.Deserialize(File.ReadAllText(savePath), SaveDataType);
        }
        else 
        {
            InternalSaveData = (ModuleSaveData)SaveDataType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        }
    }

    public void LoadSettings()
    {
        InternalSettings = (ModuleSettings)SettingsType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());

        if (InternalSettings == null)
            return;

        var path = Path.Combine(GetRootPath(), "Saves", ID, Name + ".settings" + ".json");
        InternalSettings.Load(path);
    }

    public void SaveSettings()
    {
        if (InternalSettings == null)
            return;
        var path = Path.Combine(GetRootPath(), "Saves", ID, Name + ".settings" + ".json");
        InternalSettings.Save(path);
    }

    public virtual void CreateModSettings(TextContainer textContainer) {}

    internal void CreateSettings(TextContainer textContainer)
    {
        CreateModSettings(textContainer);

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
                var defaultVal = (bool)field.GetValue(settings);
                var toggleable = new TextContainer.Toggleable(fullName, defaultVal);
                toggleable.Change(x => {
                    field.SetValue(settings, x);
                });
                textContainer.Add(toggleable);
            }
            else if (fieldType == typeof(Action))
            {
                var actionButton = new TextContainer.ButtonText(fullName);
                actionButton.OnConfirm = () => {
                    var action = (Action)field.GetValue(settings);
                    action?.Invoke();
                };
                textContainer.Add(actionButton);
            }
            else if ((fieldType == typeof(int)) && (optAttrib = field.GetCustomAttribute<SettingsOptionsAttribute>()) != null)
            {
                var defaultVal = (int)field.GetValue(settings);
                var selectionOption = new TextContainer.SelectionOption(fullName, optAttrib.Options, defaultVal);
                selectionOption.Change(x => {
                    field.SetValue(settings, x.Item2);
                });
                textContainer.Add(selectionOption);
            }
            else if ((fieldType == typeof(string)) && (optAttrib = field.GetCustomAttribute<SettingsOptionsAttribute>()) != null)
            {
                var defaultVal = (string)field.GetValue(settings);
                var selectionOption = new TextContainer.SelectionOption(
                    fullName, optAttrib.Options, Array.IndexOf<string>(optAttrib.Options, defaultVal));
                selectionOption.Change(x => {
                    field.SetValue(settings, x.Item1);
                });
                textContainer.Add(selectionOption);
            }
            else if ((fieldType == typeof(int) || fieldType == typeof(float)) &&
                (attrib = field.GetCustomAttribute<SettingsNumberAttribute>()) != null)
            {
                var defaultVal = (int)field.GetValue(settings);
                var numberButton = new TextContainer.Number(fullName, defaultVal, attrib.Min, attrib.Max);
                numberButton.Change(x => {
                    if (field.FieldType == typeof(float))
                        field.SetValue(settings, (float)x);
                    else
                        field.SetValue(settings, x);
                });

                textContainer.Add(numberButton);
            }
        }
    }

    /// <summary>
    /// Override this function and this is called after all mods are loaded and
    /// this mod is registered.
    /// </summary>
    public virtual void AfterLoad() {}
    /// <summary>
    /// Override this function and load your contents here such as <see cref="Monocle.Atlas"/>,
    /// <see cref="Monocle.SFX"/>, <see cref="Monocle.SpriteData"/>, etc.. <br/>
    /// There is <see cref="FortModule.Content"/> you can use to load your content inside of your mod folder or zip.
    /// </summary>
    public virtual void LoadContent() {}
    /// <summary>
    /// Override this function and this is called after all the game data is loaded.
    /// </summary>
    public virtual void Initialize() {}


    /// <summary>
    /// Override this function and allows you to add your own variant using the <paramref name="manager"/>.
    /// </summary>
    /// <param name="manager">A <see cref="FortRise.VariantManager"/> use to add variant</param>
    /// <param name="noPerPlayer">Checks if the variant would not a per player variant, default is true</param>
    public virtual void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false) {}
    /// <summary>
    /// Override this function and allows you to parse a launch arguments that has been passed to the game.
    /// </summary>
    /// <param name="args">A launch arguments that has been passed to the game</param>
    public virtual void ParseArgs(string[] args)
    {
    }

    /// <inheritdoc cref="RiseCore.IsModExists(string)"/>
    public bool IsModExists(string modName)
    {
        return RiseCore.IsModExists(modName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetRootPath()
    {
        return ModIO.GetRootPath();
    }

    public IModRegistry GetOtherRegistry(string modName)
    {
        return RiseCore.ModuleManager.GetRegistry(modName);
    }

    public IModRegistry GetOtherRegistry(ModuleMetadata metadata)
    {
        return RiseCore.ModuleManager.AddOrGetRegistry(metadata);
    }
}