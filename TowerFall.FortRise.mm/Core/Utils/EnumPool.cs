using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FortRise;

public static class EnumPool 
{
    private class Container<T> where T : struct, INumber<T> 
    {
        private Dictionary<Type, HashSet<T>> obtained = new Dictionary<Type, HashSet<T>>();
        private Dictionary<Type, HashSet<T>> freed = new Dictionary<Type, HashSet<T>>();
        private Dictionary<Type, T> next = new Dictionary<Type, T>();

        public TEnum Obtain<TEnum>()
        where TEnum : struct, Enum
        {
            Type type = typeof(TEnum);
            if (freed.Count != 0 && freed.TryGetValue(type, out var set))
            {
                var first = set.First();
                set.Remove(first);
                return (TEnum)(object)first;
            }

            ref var nextCase = ref CollectionsMarshal.GetValueRefOrAddDefault(next, type, out bool nextExists);
            ref var obtainedHashset = ref CollectionsMarshal.GetValueRefOrAddDefault(obtained, type, out bool obtainExists);

            if (!nextExists)
            {
                var maxValue = (T)(object)Enum.GetValues<TEnum>().Max();
                maxValue += T.One;
                nextCase = maxValue;
            }
            else 
            {
                nextCase += T.One;
            }

            if (!obtainExists)
            {
                obtainedHashset = [];
            }

            obtainedHashset.Add(nextCase);
            return (TEnum)(object)nextCase;
        }

        public void Free<TEnum>(TEnum value)
        where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            if (!obtained.TryGetValue(type, out var obtainedHashset))
            {
                return;
            }

            var num = (T)(object)value;

            if (!obtainedHashset.Remove(num))
            {
                return;
            }

            ref var freeHashset = ref CollectionsMarshal.GetValueRefOrAddDefault(freed, type, out bool freeExists);
            if (!freeExists)
            {
                freeHashset = [];
            }
            freeHashset.Add(num);
        }
    }

    private static readonly Container<int> Int = new();
    private static readonly Container<int> UInt = new();
    private static readonly Container<int> Long = new();
    private static readonly Container<int> ULong = new();
    private static readonly Container<int> Short = new();
    private static readonly Container<int> UShort = new();
    private static readonly Container<int> Byte = new();
    private static readonly Container<int> SByte = new();

    public static T Obtain<T>()
    where T : struct, Enum
    {
        var type = Enum.GetUnderlyingType(typeof(T));
        if (type == typeof(int))
        {
            return Int.Obtain<T>();
        }
        if (type == typeof(uint))
        {
            return UInt.Obtain<T>();
        }
        if (type == typeof(long))
        {
            return Long.Obtain<T>();
        }
        if (type == typeof(ulong))
        {
            return ULong.Obtain<T>();
        }
        if (type == typeof(short))
        {
            return Short.Obtain<T>();
        }
        if (type == typeof(ushort))
        {
            return UShort.Obtain<T>();
        }
        if (type == typeof(byte))
        {
            return Byte.Obtain<T>();
        }
        if (type == typeof(sbyte))
        {
            return SByte.Obtain<T>();
        }
        throw new UnreachableException();
    }

    public static void Free<T>(T value)
    where T : struct, Enum
    {
        var type = Enum.GetUnderlyingType(typeof(T));
        if (type == typeof(int))
        {
            Int.Free(value);
            return;
        }
        if (type == typeof(uint))
        {
            UInt.Free(value);
            return;
        }
        if (type == typeof(long))
        {
            Long.Free(value);
            return;
        }
        if (type == typeof(ulong))
        {
            ULong.Free(value);
            return;
        }
        if (type == typeof(short))
        {
            Short.Free(value);
            return;
        }
        if (type == typeof(ushort))
        {
            UShort.Free(value);
            return;
        }
        if (type == typeof(byte))
        {
            Byte.Free(value);
            return;
        }
        if (type == typeof(sbyte))
        {
            SByte.Free(value);
            return;
        }
        throw new UnreachableException();
    }
}