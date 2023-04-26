using System;
using System.Xml;

namespace Monocle;

public static class patch_Calc 
{
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