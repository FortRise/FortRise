using Microsoft.Xna.Framework;
using TeuJson;

namespace TowerFall;

public static class JsonUtils 
{
    public static Option<JsonValue> GetJsonValueOrNone(this JsonValue value, string key) 
    {
        if (value.Contains(key))
            return Option<JsonValue>.Some(value[key]);
        return Option<JsonValue>.None();
    }

    public static JsonValue GetJsonValueOrNull(this JsonValue value, string key) 
    {
        if (value.Contains(key))
            return value[key];
        return null;
    }

    public static Vector2 Position(this JsonValue value) 
    {
        return new Vector2(value["x"], value["y"]);
    }
}

public class Option<T> 
{
    public T Wrapped;

    private Option(T value) 
    {
        Wrapped = value;
    }

    public bool Unwrap(out T value) 
    {
        if (Wrapped != null) 
        {
            value = Wrapped;
            return true;
        }
        value = default;
        return false;
    }

    public static Option<T> None() 
    {
        return new Option<T>(default);
    }

    public static Option<T> Some(T value) 
    {
        return new Option<T>(value);
    }
}