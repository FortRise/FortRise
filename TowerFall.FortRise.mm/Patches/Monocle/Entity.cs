using System.Collections.Generic;
using FortRise;
using MonoMod;

namespace Monocle;

public class patch_Entity : Entity
{
    public void Add<T>(List<T> components)
    where T : Component
    {
        foreach (var component in components)
        {
            Add(component);
        }
    }

    public void Remove<T>(List<T> components)
    where T : Component
    {
        foreach (var component in components)
        {
            Remove(component);
        }
    }

    [MonoModReplace]
    public void RemoveSelf() 
    {
        if (Scene != null)
        {
            Scene.Remove(this);
        }
        else 
        {
            Logger.Error($"Entity: {GetType().FullName} being removed without a scene.");
        }
    }
}

public static class EntityEx
{
    extension(Entity self)
    {
        public void Add<T>(List<T> components)
        where T : Component
        {
            (self as patch_Entity).Add(components);
        }

        public void Remove<T>(List<T> components)
        where T : Component
        {
            (self as patch_Entity).Remove(components);
        }
    }
}