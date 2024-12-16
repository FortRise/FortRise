using System;
using Microsoft.Xna.Framework;
using TeuJson;

namespace FortRise;

public static class JsonUtils 
{
    public static Hjson.JsonValue GetJsonValueOrNull(this Hjson.JsonValue value, string key) 
    {
        if (value.ContainsKey(key))
            return value[key];
        return null;
    }

    public static bool TryParseEnum<T>(this Hjson.JsonValue value, string key, out T result) 
    where T : struct, Enum
    {
        if (value.ContainsKey(key)) 
        {
            var str = value[key];
            if (Enum.TryParse<T>(str, out result))
                return true;

            return false;
        }
        result = default;
        return false;
    }

    public static bool TryGetValue(this Hjson.JsonValue value, string key, out Hjson.JsonValue result) 
    {
        if (value.ContainsKey(key)) 
        {
            result = value[key];
            return true;
        }
        result = null;
        return false;
    }

    public static Vector2 Position(this Hjson.JsonValue value) 
    {
        return new Vector2(value["x"], value["y"]);
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

    public static bool TryParseEnum<T>(this JsonObject value, string key, out T result) 
    where T : struct, Enum
    {
        if (value.Contains(key)) 
        {
            var str = value[key];
            if (Enum.TryParse<T>(str, out result))
                return true;

            return false;
        }
        result = default;
        return false;
    }

    public static bool TryGetValue(this JsonObject value, string key, out JsonValue result) 
    {
        if (value.Contains(key)) 
        {
            result = value[key];
            return true;
        }
        result = null;
        return false;
    }
}