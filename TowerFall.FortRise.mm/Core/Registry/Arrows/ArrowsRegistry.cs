#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Reflection;
using Monocle;
using TowerFall;

namespace FortRise;

public static class ArrowsRegistry
{
    public static Dictionary<ArrowTypes, ArrowData> ArrowDatas = new Dictionary<ArrowTypes, ArrowData>();
    public static Dictionary<Type, ArrowTypes> Types = new();
    public static Dictionary<string, ArrowTypes> StringToTypes = new();


    public static void Register<T>(FortModule module) 
    {
        Register(typeof(T), module);
    }

    public static void Register(Type type, FortModule module) 
    {
        foreach (var arrow in type.GetCustomAttributes<CustomArrowsAttribute>()) 
        {
            if (type is null)
                return;
            var name = arrow.Name ?? $"{type.Namespace}.{type.Name}";
            var graphicFn = arrow.CreateHud ?? "CreateHud";
            MethodInfo graphic = type.GetMethod(graphicFn);
            string id = $"{module.Meta.Name}/{name}";
            Register(id, new() 
            {
                ArrowType = type,
                HUD = graphic.Invoke(null, []) as Subtexture
            });
        }
    }

    public static void Register(string name, in ArrowConfiguration configuration)
    {
        const int offset = 11;
        var stride = (ArrowTypes)offset + ArrowDatas.Count;
        var type = configuration.ArrowType;
        ConstructorInfo ctor = type.GetConstructor([]);
        ArrowLoader loader = null;
        if (ctor != null) 
        {
            loader = () => 
            {
                var invoked = (patch_Arrow)ctor.Invoke([]);
                invoked.ArrowType = stride;
                return invoked;
            };
        }

        StringToTypes[name] = stride;

        ArrowDatas[stride] = new ArrowData() 
        {
            ArrowLoader = loader,
            Types = stride,
            Hud = configuration.HUD,
            Name = name,
            ArrowType = configuration.ArrowType
        };
        Types.Add(type, stride);

    }
}