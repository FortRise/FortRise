using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FortRise;

public static class RiseCore 
{
    private static List<Assembly> ModAssemblies = new List<Assembly>();

    public readonly static Type[] EmptyTypeArray = new Type[0];
    public readonly static object[] EmptyObjectArray = new object[0];

    internal static void ModuleStart() 
    {
        try 
        {
            var allLines = File.ReadAllLines("Mods/ModList.txt");
            for (int i = 0; i < allLines.Length; i++) 
            {
                if (allLines[i] == string.Empty) { continue; }
                Assembly asm = Assembly.LoadFile("Mods/" + allLines[i]);
                ModAssemblies.Add(asm);
                Type t = asm.GetType("Entry");
                var obj = Activator.CreateInstance(t);
                var method = t.GetMethod("Load");
                method.Invoke(obj, null);
            }
        }
        catch (Exception) 
        {

        }
    }

    internal static void LoadContent() 
    {
        foreach (var modAssembly in ModAssemblies) 
        {
            Type t = modAssembly.GetType("Entry");
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("LoadContent");
            method.Invoke(obj, null);
        }
    }

    internal static void Initialize() 
    {
        foreach (var modAssembly in ModAssemblies) 
        {
            Type t = modAssembly.GetType("Entry");
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("Initialize");
            method.Invoke(obj, null);
        }
    }

    internal static void ModuleEnd() 
    {
        foreach (var modAssembly in ModAssemblies) 
        {
            Type t = modAssembly.GetType("Entry");
            var obj = Activator.CreateInstance(t);
            var method = t.GetMethod("Unload");
            method.Invoke(obj, null);
        }
    }

    private static void CallModuleMethod(string methodName) 
    {
        if (ModAssemblies.Count < 1) { return; }
        foreach (var assembly in ModAssemblies) 
        {
            Type t = assembly.GetType("Entry");
            object obj = Activator.CreateInstance(t);
            var method = t.GetMethod(methodName);
            method.Invoke(obj, null);
        }
    }
}

// Work In Progress
public class ModuleHandler
{
    public Assembly ModuleAssembly;
    public object Instance;
    private Type type;

    public ModuleHandler(Assembly assembly, object instance)  
    {
        ModuleAssembly = assembly;
        Instance = instance;
        type = ModuleAssembly.GetType("Entry");
    }

    public void InvokeMethod(string methodName) 
    {
        var method = type.GetMethod(methodName);
        method.Invoke(Instance, null);
    }
}