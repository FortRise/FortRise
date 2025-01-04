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
            var graphicFn = arrow.GraphicPickupInitializer ?? "CreateGraphicPickup";
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
            ArrowInfoLoader infoLoader = null;
            if (graphic == null || !graphic.IsStatic)
            {
                Logger.Warning($"[Loader] [{module.Meta.Name}] No `static ArrowInfo CreateGraphicPickup()` method found on this Arrow {name}, falling back to normal arrow graphics.");
                infoLoader = () => {
                    return ArrowInfo.Create(new Image(TFGame.Atlas["arrows/arrow"]));
                };
            }
            else 
            {
                infoLoader = () => {
                    var identifier = (ArrowInfo)graphic.Invoke(null, Array.Empty<object>());
                    if (string.IsNullOrEmpty(identifier.Name))
                        identifier.Name = name;
                    
                    var trimmedName = identifier.Name.Trim();
                    if (trimmedName.EndsWith("Arrows") || trimmedName.EndsWith("Arrow")) 
                    {
                        trimmedName = trimmedName.Replace("Arrows", "").Trim();
                    }
                    identifier.Name = trimmedName;
                    return identifier;
                };
            }

            Pickups stridePickup = (Pickups)21 + PickupsRegistry.PickupDatas.Count; 
            var pickupObject = new PickupData() 
            {
                Name = name,
                ID = stridePickup,
                Chance = arrow.Chance,
                PickupLoader = (pos, targetPos, _) => new ArrowTypePickup(pos, targetPos, stride)
            };

            ArrowDatas[stride] = new ArrowData() 
            {
                ArrowLoader = loader,
                Types = stride,
                InfoLoader = infoLoader,
                PickupType = pickupObject
            };
            StringToTypes[name] = stride;

            PickupsRegistry.StringToTypes[name] = stridePickup;
            PickupsRegistry.PickupDatas[stridePickup] = pickupObject;
            PickupsRegistry.Types.Add(type, stridePickup);
            Types.Add(type, stride);
        }
    }
}