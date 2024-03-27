using System;

namespace FortRise;

public sealed class CustomDarkWorldBossAttribute : Attribute 
{
    public string BossName;
    public CustomDarkWorldBossAttribute(string bossName) 
    {
        BossName = bossName;
    }
}