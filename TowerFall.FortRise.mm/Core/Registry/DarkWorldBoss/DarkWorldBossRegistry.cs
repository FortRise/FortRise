#nullable enable
using System.Collections.Generic;

namespace FortRise;

public static class DarkWorldBossRegistry
{
    public static Dictionary<string, int> DarkWorldBosses = new Dictionary<string, int>();
    public static Dictionary<int, DarkWorldBossLoader?> DarkWorldBossLoader = new Dictionary<int, DarkWorldBossLoader?>();
}
