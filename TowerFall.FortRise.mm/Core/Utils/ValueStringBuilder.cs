using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace FortRise;

public ref struct ValueStringBuilder : IDisposable
{
    private int bufferPosition;
    private Span<char> buffer;
    private char[] pooledArray;

    public ref char this[int index] => ref buffer[index];
    public readonly int Capacity => buffer.Length;

    public ValueStringBuilder()
    {
        Grow(32);
    }

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        buffer = initialBuffer;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(T value, ReadOnlySpan<char> format, int bufferSize = 36)
    where T : ISpanFormattable
    {
        AppendFormattable(value, format, bufferSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        var newSize = bufferPosition + 1;
        if (newSize > buffer.Length)
        {
            Grow(newSize);
        }

        buffer[bufferPosition] = c;
        bufferPosition += 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> str)
    {
        var newSize = str.Length + bufferPosition;
        if (newSize > buffer.Length)
        {
            Grow(newSize);
        }

        // copy the string to the buffer within the bufferPosition
        str.CopyTo(buffer[bufferPosition..]);
        bufferPosition += str.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine()
    {
        Append(Environment.NewLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(ReadOnlySpan<char> str)
    {
        Append(str);
        Append(Environment.NewLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormattable<T>(T value, ReadOnlySpan<char> format = default, int bufferSize = 36)
    where T : ISpanFormattable
    {
        var newSize = bufferSize + bufferPosition;
        if (newSize >= Capacity)
        {
            Grow(newSize);
        }

        if (!value.TryFormat(buffer[bufferPosition..], out int written, format, null))
        {
            throw new InvalidOperationException($"Cannot insert {value} into given buffer size: {bufferSize}.");
        }

        bufferPosition += written;
    }

    public readonly ReadOnlySpan<char> AsSpan()
    {
        return buffer[..bufferPosition];
    }

    public override readonly string ToString()
    {
        return new string(buffer[..bufferPosition]);
    }

    public void Clear()
    {
        bufferPosition = 0;
    }

    private void Grow(int newCapacity = 0)
    {
        if (Capacity >= newCapacity)
        {
            return;
        }

        int newSize = 1 << (int)Math.Ceiling(Math.Log2(newCapacity));

        var rented = ArrayPool<char>.Shared.Rent(newSize);
        buffer.CopyTo(rented);

        if (pooledArray != null)
        {
            ArrayPool<char>.Shared.Return(pooledArray);
        }

        buffer = rented;
        pooledArray = rented;
    }

    public readonly void Dispose()
    {
        if (pooledArray != null)
        {
            ArrayPool<char>.Shared.Return(pooledArray);
        }
    }
}