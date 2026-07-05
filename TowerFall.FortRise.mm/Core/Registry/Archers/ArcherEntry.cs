#nullable enable
using System;
using TowerFall;

namespace FortRise;

internal sealed class ArcherEntry : IArcherEntry
{
    public string Name { get; init; }
    public ArcherConfiguration Configuration { get; init; }
    public int Index { get; init; }
    public ArcherEntryType Type { get; init; }
    public ArcherData? ArcherData
    {
        get
        {
            try
            {
                switch (Type)
                {
                    case ArcherEntryType.Normal:
                        return ArcherData.Archers[Index];
                    case ArcherEntryType.Alt:
                        return ArcherData.AltArchers[Index];
                    case ArcherEntryType.Secret:
                        return ArcherData.SecretArchers[Index];
                    default:
                        return null;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
    }

    public ArcherEntry(string name, ArcherConfiguration configuration, int index, ArcherEntryType entryType)
    {
        Name = name;
        Configuration = configuration;
        Index = index;
        Type = entryType;
    }
}
