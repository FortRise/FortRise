using System;
using System.Xml;
using System.Reflection;

namespace Monocle;

public static class patch_Calc 
{
    public static Delegate GetMethod<T>(object obj, string method) where T : class 
    {
        if (obj.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
        {
            return Delegate.CreateDelegate(typeof(T), obj, method);
        }
        if (obj.GetType().BaseType
            .GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
        {
            return Delegate.CreateDelegate(typeof(T), obj, method);
        }
        return null;
    }

    public static T[] ChildEnumArray<T>(this XmlElement xml, string childName)
    where T : struct
    {
        var childs = xml[childName].InnerText.Split();
        if (childs == null)
            return Array.Empty<T>();
        var array = new T[childs.Length];
        int i = 0;
        foreach (var child in childs) 
        {
            if (System.Enum.TryParse<T>(child, out T result)) 
            {
                array[i] = result;
            }
            i++;
        }
        return array;
    }
}