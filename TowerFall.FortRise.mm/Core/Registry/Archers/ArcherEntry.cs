#nullable enable
using System;
using TowerFall;

namespace FortRise;

internal sealed class ArcherEntry : IArcherEntry
{
    public string Name { get; init; }
    public ArcherConfiguration Configuration { get; init; }
    private int index;
    public int Index { get => index; init => index = value; }
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

    // archers after a removed one move down a slot
    internal void SetIndex(int newIndex) => index = newIndex;

    public ArcherEntry(string name, ArcherConfiguration configuration, int index, ArcherEntryType entryType)
    {
        Name = name;
        Configuration = configuration;
        Index = index;
        Type = entryType;
    }
}
