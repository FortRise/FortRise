using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

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


    // WTH, why?
    [MonoModIgnore]
    private extern static patch_ActorData AddData(string name, string title, Subtexture subtexture, Vector2 origin, int width, int height, bool allowScreenWrap, bool hasNode, bool resizeableX, bool resizeableY, int minWidth, int maxWidth, int minHeight, int maxHeight, int weight, bool darkWorldDLC, Action<Actor, Vector2, float> renderer = null);
}