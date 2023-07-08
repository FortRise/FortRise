using System;
using System.IO;
using Microsoft.Xna.Framework;
using NLua;

namespace FortRise;


// TODO Need more functionality, can be used for editor maybe?
public static partial class RiseCore
{
    public static class Lua 
    {
        public static NLua.Lua Context { get; private set; }

        internal static void Initialize() 
        {
            Context = new NLua.Lua();
        }

        public static NLua.LuaFunction LoadScript(string path) 
        {
            if (!File.Exists(path))
                return null;
            using var fs = File.OpenRead(path);
            return LoadScript(fs, Path.GetFileNameWithoutExtension(path));
        }

        public static NLua.LuaFunction LoadScript(Stream stream, string chunk) 
        {
            using TextReader sr = new StreamReader(stream);
            var texts = sr.ReadToEnd();
            return Context.LoadString(texts, chunk);
        }
    }
}

public static class LuaHelper 
{
    public static string Get(this LuaTable table, string name) 
    {
        return table[name] as string;
    }

    public static string Get(this LuaTable table, string name, string defaultValue) 
    {
        var value = table[name] as string;
        if (value == null)
            return defaultValue;
        return value;
    }

    public static bool TryGet(this LuaTable table, string name, out string result) 
    {
        var val = table[name] as string;
        if (val != null)
        {
            result = val;
            return true;
        }
        result = null;
        return false;
    }

    public static string[] GetStringArray(this LuaTable xml, string childName)
    {
        if (!xml.TryGet(childName, out var result))
        {
            return null;
        }
        string[] array = result.Split(new char[] { ',' });
        if (array == null)
        {
            return Array.Empty<string>();
        }
        string[] array2 = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array2[i] = array[i];
        }
        return array2;
    }

    public static bool Contains(this LuaTable table, string name) 
    {
        return (table[name]) != null;
    }

    public static T GetEnum<T>(this LuaTable table, string childName) where T : struct
    {
        var value = table.Get(childName);
        if (System.Enum.IsDefined(typeof(T), value))
        {
            return (T)((object)System.Enum.Parse(typeof(T), value));
        }
        throw new System.Exception("The attribute value cannot be converted to the enum type.");
    }

    public static T GetEnum<T>(this LuaTable table, string childName, T defaultValue) where T : struct
    {
        var value = table.Get(childName);
        if (value == null)
        {
            return defaultValue;
        }
        if (System.Enum.IsDefined(typeof(T), value))
        {
            return (T)((object)System.Enum.Parse(typeof(T), value));
        }
        throw new System.Exception("The attribute value cannot be converted to the enum type.");
    }


    public static bool GetBool(this LuaTable table, string name) 
    {
        object val = table[name];
        if (val is bool b)
            return b;
        return false;
    }

    public static bool GetBool(this LuaTable table, string name, bool defaultValue) 
    {
        object val = table[name];
        if (val is bool b)
            return b;
        return defaultValue;
    }

    public static bool TryGetBool(this LuaTable table, string name, out bool result) 
    {
        var val = table[name];
        if (val is bool b)
        {
            result = b;
            return true;
        }
        result = false;
        return false;
    }

    public static int GetInt(this LuaTable table, string name) 
    {
        object val = table[name];
        if (val is long i)
            return (int)i;
        return 0;
    }

    public static int GetInt(this LuaTable table, string name, int defaultValue) 
    {
        object val = table[name];
        if (val is long i)
            return (int)i;
        return defaultValue;
    }

    public static bool TryGetInt(this LuaTable table, string name, out int result) 
    {
        var val = table[name];
        if (val is long i)
        {
            result = (int)i;
            return true;
        }
        result = 0;
        return false;
    }

    public static float GetFloat(this LuaTable table, string name) 
    {
        object val = table[name];
        if (val is double f)
            return (float)f;
        return 0f;
    }

    public static float GetFloat(this LuaTable table, string name, float defaultValue) 
    {
        object val = table[name];
        if (val is double f)
            return (float)f;
        return defaultValue;
    }

    public static LuaTable GetTable(this LuaTable table, string name) 
    {
        return table[name] as LuaTable;
    }


    public static bool TryGetTable(this LuaTable table, string name, out LuaTable result) 
    {
        var val = table[name] as LuaTable;
        if (val != null)
        {
            result = val;
            return true;
        }
        result = null;
        return false;
    }

    public static Vector2 Position(this LuaTable table) 
    {
        var x = (long)table["x"];
        var y = (long)table["y"];
        return new Vector2(x, y);
    }
}