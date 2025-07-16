#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace FortRise;

public static class Private
{
    public static unsafe FastPrivateAccess<TReturn> Field<TBase, TReturn>(string name, TBase? instance)
    {
        throw new InvalidOperationException("This method cannot be called without a 'FortRise.Generator' or 'FortRise.Configuration'");
    }
}

public readonly unsafe ref struct FastPrivateAccess<TReturn>(void * fieldAccess)
{
    private readonly void* fieldAccess = fieldAccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Write(TReturn value)
    {
        Unsafe.Write(fieldAccess, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TReturn Read()
    {
        return Unsafe.Read<TReturn>(fieldAccess);
    }
}