using System;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise.Bugfixes;

internal sealed class BugfixesSettings : ModuleSettings
{
    public bool AsymmetricalRayGridCheckFix { get; set; } = true;

    public override void Create(ISettingsCreate settings)
    {
        settings.CreateOnOff("Fix Asymmetrical Ray Grid Check", AsymmetricalRayGridCheckFix, (x) => AsymmetricalRayGridCheckFix = x);
    }
}

internal sealed class BugfixesModule : Mod
{
    public static BugfixesModule Instance { get; private set; } = null!;
    public static BugfixesSettings Settings => Instance.GetSettings<BugfixesSettings>()!;

    public BugfixesModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;
        context.Harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Grid), nameof(Grid.Collide), [typeof(Vector2), typeof(Vector2)]),
            prefix: new HarmonyMethod(Grid_CollideLine_Prefix)
        );
    }

    public override object GetApi()
    {
        return new ApiImplementation();
    }

    public override ModuleSettings CreateSettings()
    {
        return new BugfixesSettings();
    }

    private static bool Grid_CollideLine_Prefix(Grid __instance, Vector2 from, Vector2 to, ref bool __result)
    {
        if (!Settings.AsymmetricalRayGridCheckFix)
        {
            return true;
        }

        from -= __instance.AbsolutePosition;
        to -= __instance.AbsolutePosition;
        from /= new Vector2(__instance.CellWidth, __instance.CellHeight);
        to /= new Vector2(__instance.CellWidth, __instance.CellHeight);

        bool steep = Math.Abs(to.Y - from.Y) > Math.Abs(to.X - from.X);
        if (steep)
        {
            float temp = from.X;
            from.X = from.Y;
            from.Y = temp;
            temp = to.X;
            to.X = to.Y;
            to.Y = temp;
        }

        if (from.X > to.X)
        {
            (to, from) = (from, to);
        }

        float slope = to.X > from.X ? (to.Y - from.Y) / (to.X - from.X) : 0f;
        int end = (int)MathF.Floor(to.X);

        for (int i = (int)MathF.Floor(from.X); i <= end; i++)
        {
            float y1 = from.Y + (MathF.Max(from.X, i) - from.X) * slope;
            float y2 = from.Y + (MathF.Min(to.X, i + 1) - from.X) * slope;
            int rowEnd = (int)MathF.Floor(MathF.Max(y1, y2));

            for (int row = (int)MathF.Floor(MathF.Min(y1, y2)); row <= rowEnd; row++)
            {
                if (steep)
                {
                    if (__instance[row, i])
                    {
                        __result = true;
                        return false;
                    }
                }
                else if (__instance[i, row])
                {
                    __result = true;
                    return false;
                }
            }
        }

        __result = false;
        return false;
    }
}
