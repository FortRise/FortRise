using System;
using System.Runtime.CompilerServices;

namespace FortRise;

public readonly struct Either<T1, T2>
{
    public readonly T1 AsValue1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => asValue1;
    }

    public readonly T2 AsValue2
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => asValue2;
    }

    public readonly object Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Index switch
        {
            0 => asValue1,
            1 => asValue2,
            _ => throw new InvalidOperationException()
        };
    }

    public int Index { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; init; }



    private readonly T1 asValue1;
    private readonly T2 asValue2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either(T1 value)
    {
        asValue1 = value;
        Index = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Either(T2 value)
    {
        asValue2 = value;
        Index = 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Switch(Action<T1> act, Action<T2> act2)
    {
        switch (Index)
        {
        case 0: act.Invoke(asValue1); break;
        case 1: act2.Invoke(asValue2); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TResult Match<TResult>(Func<T1, TResult> res1, Func<T2, TResult> res2)
    {
        return Index switch 
        {
            0 => res1(asValue1),
            1 => res2(asValue2),
            _ => throw new InvalidOperationException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Either<T1, T2>(T1 value)
    {
        return new Either<T1, T2>(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Either<T1, T2>(T2 value)
    {
        return new Either<T1, T2>(value);
    }
}


