using System;

namespace FortRise.Adventure;

[Fort("com.fortrise.adventure", "Adventure")]
public sealed class AdventureModule : FortModule
{
    public static AdventureModule Instance;

    public override Type SaveDataType => typeof(AdventureModuleSaveData);
    public static AdventureModuleSaveData SaveData => (AdventureModuleSaveData)Instance.InternalSaveData;

    public AdventureModule()
    {
        Instance = this;

        // Internal things
        Meta = new ModuleMetadata() {
            Name = "Adventure",
            Author = "FortRise",
            Version = new Version("2.0.0"),
        };

        Name = "Adventure";
        ID = "com.fortrise.adventure";
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
    }
}