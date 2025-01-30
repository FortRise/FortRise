using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace FortRise;

public static class JsonUtils 
{
    public static string GetStringOrNull(this Dictionary<string, JsonElement> value, string key) 
    {
        if (value.TryGetValue(key, out var v))
        {
            return v.GetString();
        }
        return null;
    }

    public static int? GetInt32OrNull(this Dictionary<string, JsonElement> value, string key) 
    {
        if (value.TryGetValue(key, out var v))
        {
            return v.GetInt32();
        }
        return null;
    }

    public static bool? GetBooleanOrNull(this Dictionary<string, JsonElement> value, string key) 
    {
        if (value.TryGetValue(key, out var v))
        {
            return v.GetBoolean();
        }
        return null;
    }

    public static float? GetSingleOrNull(this Dictionary<string, JsonElement> value, string key) 
    {
        if (value.TryGetValue(key, out var v))
        {
            return v.GetSingle();
        }
        return null;
    }

    public static Vector2 Position(this Dictionary<string, JsonElement> value) 
    {
        return new Vector2(value["x"].GetSingle(), value["y"].GetSingle());
    }

    public static bool TryParseEnum<T>(this Dictionary<string, JsonElement> value, string key, out T result) 
    where T : struct, Enum
    {
        if (value.TryGetValue(key, out var str))
        {
            if (Enum.TryParse<T>(str.GetString(), out result))
                return true;

            return false;
        }
        result = default;
        return false;
    }
}