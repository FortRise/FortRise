#nullable enable
using System;
using System.Collections.Generic;

namespace FortRise;

// During a hot reload, slots (indices, enum values, IDs) are kept by name so a re-registered entry keeps its identity.
public sealed class IndexReservationPool
{
    private readonly HashSet<string> mods;
    private readonly Dictionary<(string Kind, string Name), (object? Slot, Action Release)> reservations = [];

    public IndexReservationPool(IEnumerable<string> modNames)
    {
        mods = [.. modNames];
    }

    public static IndexReservationPool? For(ModuleMetadata metadata)
    {
        var session = ModuleManager.Instance.IDReservationPool;
        return session is not null && session.mods.Contains(metadata.Name) ? session : null;
    }

    public static TEnum ClaimOrObtain<TEnum>(ModuleMetadata metadata, string name)
    where TEnum : struct, Enum
    {
        return For(metadata) is { } session && session.TryClaim(typeof(TEnum).Name, name, out TEnum value)
            ? value
            : EnumPool.Obtain<TEnum>();
    }

    public static void FreeOrReserve<TEnum>(ModuleMetadata metadata, string name, TEnum value)
    where TEnum : struct, Enum
    {
        if (For(metadata) is { } session)
        {
            session.Reserve(typeof(TEnum).Name, name, value, () => EnumPool.Free(value));
            return;
        }

        EnumPool.Free(value);
    }

    public void Reserve<TSlot>(string kind, string name, TSlot slot, Action release)
    {
        reservations[(kind, name)] = (slot, release);
    }

    public bool IsReserved(string kind, string name) => reservations.ContainsKey((kind, name));

    public bool TryClaim<TSlot>(string kind, string name, out TSlot slot)
    {
        if (reservations.TryGetValue((kind, name), out var reservation) && reservation.Slot is TSlot typed)
        {
            reservations.Remove((kind, name));
            slot = typed;
            return true;
        }

        slot = default!;
        return false;
    }

    public void ReleaseUnclaimed(Action<string, Action> guard)
    {
        foreach (var ((kind, name), (_, release)) in reservations)
        {
            guard($"releasing {kind} '{name}'", release);
        }

        reservations.Clear();
    }
}
