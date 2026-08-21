namespace MonoMod;

internal static partial class MonoModRules 
{
    private static void ModPatch(MonoModder modder)
    {
        modder.PostProcessors += ModPostProcessor;
    }

    private static void ModPostProcessor(MonoModder modder) 
    {

    }
}
