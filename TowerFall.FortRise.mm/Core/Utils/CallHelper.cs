using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FortRise;

public static class CallHelper 
{
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

    public static Action<TTarget, T1, T2, T3> CallBaseGen<TBase, TTarget, T1, T2, T3>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType, typeof(T1), typeof(T2), typeof(T3) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3>));
    }

    public static Action<TTarget, T1, T2, T3> CallBaseGen<TBase, TTarget, T1, T2, T3>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType, typeof(T1), typeof(T2), typeof(T3) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3>));
    }
}