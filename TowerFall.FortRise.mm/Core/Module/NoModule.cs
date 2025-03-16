namespace FortRise;

internal sealed class NoModule : FortModule
{
    public NoModule(ModuleMetadata metadata) 
    {
        Meta = metadata;
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
    }
}