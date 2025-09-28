using System.Runtime.CompilerServices;

namespace FortRise;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct Option<T>(T value)
{
    public T Value { get; private set; } = value;
    public bool HasValue { get; private set; } = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> None()
    {
        return new Option<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetValue(out T value)
    {
        value = Value; // No need for if checks since Value could be default anyway
        return HasValue; // only important here is the HasValue
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Option<T>(T value)
    {
        return new Option<T>(value);
    }
}
