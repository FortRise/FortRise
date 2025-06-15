using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;

namespace FortRise;

[Obsolete("Just remove it, it will still work")]
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

public abstract partial class FortModule : Mod
{
    public bool Enabled { get; internal set; }
    public bool DisposeTextureAfterUnload { get; set; } = true;
    
    [Obsolete("Use 'Meta.DisplayName' or 'Meta.Name' instead")]
    public string Name => string.IsNullOrEmpty(Meta.DisplayName) ? Meta.Name : Meta.DisplayName;
    [Obsolete("Use 'Meta.Name' instead")]
    public string ID => Meta.Name;


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

    protected FortModule(IModContent content, IModuleContext context) : base(content, context)
    {
        OnLoad += InternalLoad;
        OnInitialize += InternalInitialize;
        OnUnload += InternalUnload;
        OnLoadContent += InternalLoadContent;
    }

    protected FortModule() : this(null, null)
    {
        OnLoad += InternalLoad;
        OnInitialize += InternalInitialize;
        OnUnload += InternalUnload;
        OnLoadContent += InternalLoadContent;
    }

    public IHarmony Harmony
    {
        get
        {
            return Context.Harmony;
        }
    }

    public IModRegistry Registry
    {
        get
        {
            return Context.Registry;
        }
    }

    public IModInterop Interop 
    {
        get 
        {
            return Context.Interop;
        }
    }

    /// <summary>
    /// The module's mod content which use to load atlases, spriteDatas, SFXes, etc..
    /// </summary>
    public FortContent Content 
    {
        get;
        internal set;
    }

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

    internal void InternalLoad(IModuleContext context)
    {
        Context.Events.OnModLoadingFinished += OnAfterLoad;
        LoadSettings();
        Load();
    }

    public void OnAfterLoad(object sender, EventArgs e)
    {
        AfterLoad();
    }

    internal void InternalUnload(IModuleContext context)
    {
        Context.Events.OnModLoadingFinished -= OnAfterLoad;
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
            var savePath = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, $"{Meta.Name}.saveData.json");
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
        var savePath = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, $"{Meta.Name}.saveData.json");
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

    public override ModuleSaveData CreateSaveData()
    {
        return (ModuleSaveData)SaveDataType?.GetConstructor([]).Invoke([]);
    }

    public override ModuleSettings CreateSettings()
    {
        return (ModuleSettings)SettingsType?.GetConstructor([]).Invoke([]);
    }

#nullable enable
    public override object? GetApi() => null;
#nullable disable

    public void LoadSettings()
    {
        InternalSettings = (ModuleSettings)SettingsType?.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());

        if (InternalSettings == null)
            return;

        var path = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, Meta.Name + ".settings" + ".json");
        InternalSettings.Load(path);
    }

    public void SaveSettings()
    {
        if (InternalSettings == null)
            return;
        var path = Path.Combine(ModIO.GetRootPath(), "Saves", Meta.Name, Meta.Name + ".settings" + ".json");
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
            var isIgnored = field.GetCustomAttribute<SettingsHideAttribute>();
            if (isIgnored is not null)
            {
                continue;
            }

            var name = field.Name;
            var fieldType = field.FieldType;
            SettingsNumberAttribute attrib = null;
            SettingsOptionsAttribute optAttrib = null;
            SettingsInputAttribute inputAttrib = null;

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
                    fullName, optAttrib.Options, Array.IndexOf(optAttrib.Options, defaultVal));
                selectionOption.Change(x => {
                    field.SetValue(settings, x.Item1);
                });
                textContainer.Add(selectionOption);
            }
            else if ((fieldType == typeof(string)) && (inputAttrib = field.GetCustomAttribute<SettingsInputAttribute>()) != null)
            {
                var defaultVal = (string)field.GetValue(settings);
                var inputText = new TextContainer.InputText(fullName, defaultVal, inputAttrib.InputBehavior)
                {
                    OnInputEntered = (str) =>
                    {
                        field.SetValue(settings, str);
                    }
                };
                textContainer.Add(inputText);
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

    internal void InternalInitialize(IModuleContext context)
    {
        Initialize();
    }

    internal void InternalLoadContent(IModuleContext context, IModContent content)
    {
        LoadContent();
    }

    /// <summary>
    /// Override this function and this is called after all mods are loaded and
    /// this mod is registered.
    /// </summary>
    public virtual void AfterLoad() { }
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
    [Obsolete("Use the new 'Registry.Variants' API")]
    public virtual void OnVariantsRegister(VariantManager manager, bool noPerPlayer = false) {}


    /// <inheritdoc cref="RiseCore.IsModExists(string)"/>
    [Obsolete("FortModule could use 'Interop' instead")]
    public bool IsModExists(string modName)
    {
        return Interop.IsModExists(modName);
    }
}
