using FortRise;
using Microsoft.Extensions.Logging;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.WorkshopDiscovery")]
public class WorkshopDiscovery : TowerFall.WorkshopDiscovery
{
    [MonoModReplace]
    private static void WriteLog(string line)
    {
        RiseCore.logger.LogInformation("Workshop Message: {line}", line);
    }
}
