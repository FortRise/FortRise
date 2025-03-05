using System.Runtime.CompilerServices;

namespace FortRise;

public struct Result<T, U>
{
    public enum Response { Ok, Error }
    public T Value { get; private set; }
    public U ErrorValue { get; private set; }

    public Response ResponseType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result(T ok, U error, Response response)
    {
        Value = ok;
        ErrorValue = error;
        ResponseType = response;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Check(out T ok, out U error)
    {
        ok = Value;
        error = ErrorValue;
        return ResponseType == Response.Ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, U> Ok(T ok)
    {
        return new Result<T, U>(ok, default, Response.Ok);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, U> Error(U error)
    {
        return new Result<T, U>(default, error, Response.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, U>(T value)
    {
        return Result<T, U>.Ok(value);
    }
}