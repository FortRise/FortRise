using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FortRise;

/// <summary>
/// A utility class for calling a method.
/// </summary>
public static partial class CallHelper 
{
    /// <summary>
    /// Generate a base method to be call later.
    /// </summary>
    /// <param name="methodName">A method name</param>
    /// <typeparam name="TBase">A base type</typeparam>
    /// <typeparam name="TTarget">A target type</typeparam>
    /// <returns>A base function</returns>
    public static Action<TTarget> CallBaseGen<TBase, TTarget>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget>)dm.CreateDelegate(typeof(Action<TTarget>));
    }

    /// <summary>
    /// Generate a base method to be call later.
    /// </summary>
    /// <param name="methodName">A method name</param>
    /// <param name="flags">A binding flags for specific use cases</param> 
    /// <typeparam name="TBase">A base type</typeparam>
    /// <typeparam name="TTarget">A target type</typeparam>
    /// <returns>A base function</returns>
    public static Action<TTarget> CallBaseGen<TBase, TTarget>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget>)dm.CreateDelegate(typeof(Action<TTarget>));
    }
}