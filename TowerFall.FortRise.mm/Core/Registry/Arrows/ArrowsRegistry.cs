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
            const int offset = 11;
            if (type is null)
                return;
            var name = arrow.Name ?? $"{type.Namespace}.{type.Name}";
            var graphicFn = arrow.CreateHud ?? "CreateHud";
            var stride = (ArrowTypes)offset + ArrowDatas.Count;
            MethodInfo graphic = type.GetMethod(graphicFn);

            ConstructorInfo ctor = type.GetConstructor(Array.Empty<Type>());
            ArrowLoader loader = null;
            if (ctor != null) 
            {
                loader = () => 
                {
                    var invoked = (patch_Arrow)ctor.Invoke(Array.Empty<object>());
                    invoked.ArrowType = stride;
                    return invoked;
                };
            }
            ArrowHUDLoader infoLoader = null;
            if (graphic == null || !graphic.IsStatic)
            {
                Logger.Warning($"[Loader] [{module.Meta.Name}] No `static Subtexture CreateHud()` method found on this Arrow {name}, falling back to normal arrow graphics.");
                infoLoader = () => {
                    return TFGame.Atlas["arrows/arrow"];
                };
            }
            else 
            {
                infoLoader = () => {
                    return (Subtexture)graphic.Invoke(null, Array.Empty<object>());

                };
            }


            StringToTypes[name] = stride;

            ArrowDatas[stride] = new ArrowData() 
            {
                ArrowLoader = loader,
                Types = stride,
                HudLoader = infoLoader,
                Name = name,
            };
            Types.Add(type, stride);

        }
    }
}