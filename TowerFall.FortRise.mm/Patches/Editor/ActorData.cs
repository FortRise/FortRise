using System;
using System.Collections.Generic;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using NLua;

namespace TowerFall.Editor;

public class patch_ActorData : ActorData 
{
    public static List<Dictionary<string, ActorData>> DataLayers;
    public Dictionary<string, string> CustomData;
    public extern static void orig_Init();


    public static void Init() 
    {
        // Worst object lifetime ever, Devs why?
        if (ActorData.Data != null)
        {
            return;
        }
        DataLayers = new();
        orig_Init();
        DataLayers.Add(ActorData.Data);
        // foreach (var mods in RiseCore.InternalMods)
        // {
        //     var content = mods.Content;
        //     if (!content.TryGetValue("Content/Editor", out var editorResource))
        //         continue;
        //     foreach (var resource in editorResource.Childrens)
        //     {
        //         if (!Path.GetExtension(resource.Path).Contains("lua"))
        //             continue;
                
        //         using var stream = resource.Stream;
        //         var luaEntity = (LuaTable)RiseCore.Lua.LoadScript(stream, resource.Path).Call()[0];
        //         RegisterLuaEntity(luaEntity, content);
        //     }
        // }
        // ActorData.Data = DataLayers[0];
    }

    public static void RegisterLuaEntity(LuaTable lua, FortContent content) 
    {
        if (ActorData.Data.Count >= 36) 
        {
            var data = ActorData.Data = new Dictionary<string, ActorData>();
            DataLayers.Add(data);
        }
        // Required Attributes
        var name = lua.Get("name").Replace("/", "__");
        var atlasPath = lua.Get("atlas");
        var textureName = lua.Get("textureName");

        // Optionals
        var title = lua.Get("title", "NONAMED").ToUpperInvariant();
        var luaOrigin = lua.GetTable("origin");
        var origin = luaOrigin == null ? Vector2.Zero : luaOrigin.Position();
        var width = lua.GetInt("width", 20);
        var height = lua.GetInt("height", 20);
        var allowScreenWrap = lua.GetBool("allowScreenWrap", true);
        var hasNodes = lua.GetBool("hasNodes", false);
        var resizableX = lua.GetBool("resizableX", false);
        var resizableY = lua.GetBool("resizableY", false);
        var minWidth = lua.GetInt("minWidth", 0);
        var minHeight = lua.GetInt("minHeight", 0);
        var maxWidth = lua.GetInt("maxWidth", 0);
        var maxHeight = lua.GetInt("maxHeight", 0);
        var weight = lua.GetInt("weight", 1);
        var darkWorldDLC = lua.GetBool("darkWorldDLC", true);

        var atlas = content.LoadAtlas(atlasPath + ".xml", atlasPath + ".png");

        var actorData = patch_ActorData.AddData(
            name, title, atlas[textureName], origin, width, height, 
            allowScreenWrap, hasNodes, resizableX, resizableY, minWidth, maxWidth, minHeight, maxHeight, 
            weight, darkWorldDLC);
        actorData.CustomData = new();

        if (!lua.TryGetTable("data", out var luaData) && luaData == null)
            return;

        var objDicts = RiseCore.Lua.Context.GetTableDict(luaData);
        foreach (var obj in objDicts) 
        {
            actorData.CustomData.Add(obj.Key.ToString(), obj.Value.ToString());
        }
    }

    // WTH, why?
    [MonoModIgnore]
    private extern static patch_ActorData AddData(string name, string title, Subtexture subtexture, Vector2 origin, int width, int height, bool allowScreenWrap, bool hasNode, bool resizeableX, bool resizeableY, int minWidth, int maxWidth, int minHeight, int maxHeight, int weight, bool darkWorldDLC, Action<Actor, Vector2, float> renderer = null);
}