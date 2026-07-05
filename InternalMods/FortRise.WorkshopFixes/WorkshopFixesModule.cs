using System;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using TowerFall;

namespace FortRise.WorkshopFixes;

internal sealed class WorkshopFixesModule : Mod
{
    public static WorkshopFixesModule Instance { get; private set; } = null!;

    public WorkshopFixesModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        if (context.Flags.IsSteam)
        {
            context.Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(WorkshopDiscovery), "Load"),
                finalizer: new HarmonyMethod(WorkshopDiscovery_Load_Finalizer)
            );
        }
    }

    private static void WorkshopDiscovery_Load_Finalizer(Exception? __exception)
    {
        if (__exception is not null)
        {
            Instance.Logger.LogError("Exception: {ex}", __exception);
        }
    }
}
