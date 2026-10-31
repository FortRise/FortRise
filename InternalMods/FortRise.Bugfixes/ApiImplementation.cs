namespace FortRise.Bugfixes;

internal sealed class ApiImplementation : IBugfixesAPI
{
    public bool AsymmetricalRayGridCheckFix
    {
        get => BugfixesModule.Settings.AsymmetricalRayGridCheckFix;
        set => BugfixesModule.Settings.AsymmetricalRayGridCheckFix = value;
   }
}
