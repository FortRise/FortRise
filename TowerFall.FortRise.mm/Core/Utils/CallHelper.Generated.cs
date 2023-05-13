
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FortRise;

public static partial class CallHelper 
{
    public static Action<TTarget, T1> CallBaseGen<TBase, TTarget, T1>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1>)dm.CreateDelegate(typeof(Action<TTarget, T1>));
    }

    public static Action<TTarget, T1> CallBaseGen<TBase, TTarget, T1>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1>)dm.CreateDelegate(typeof(Action<TTarget, T1>));
    }
    public static Action<TTarget, T1, T2> CallBaseGen<TBase, TTarget, T1, T2>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2>));
    }

    public static Action<TTarget, T1, T2> CallBaseGen<TBase, TTarget, T1, T2>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2>));
    }
    public static Action<TTarget, T1, T2, T3> CallBaseGen<TBase, TTarget, T1, T2, T3>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3) }, targetType);
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
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3>));
    }
    public static Action<TTarget, T1, T2, T3, T4> CallBaseGen<TBase, TTarget, T1, T2, T3, T4>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4>));
    }

    public static Action<TTarget, T1, T2, T3, T4> CallBaseGen<TBase, TTarget, T1, T2, T3, T4>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5, T6> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5, T6> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Ldarg_S, 9);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Ldarg_S, 9);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9>));
    }
    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Ldarg_S, 9);       
        gen.Emit(OpCodes.Ldarg_S, 10);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>));
    }

    public static Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CallBaseGen<TBase, TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string methodName, BindingFlags flags) 
    {
        var targetType = typeof(TTarget);
        var baseUpdateMethod = typeof(TBase).GetMethod(methodName, flags);
        var dm = new DynamicMethod("<Base>" + methodName, null, new Type[] { targetType,typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) }, targetType);
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);       
        gen.Emit(OpCodes.Ldarg_S, 5);       
        gen.Emit(OpCodes.Ldarg_S, 6);       
        gen.Emit(OpCodes.Ldarg_S, 7);       
        gen.Emit(OpCodes.Ldarg_S, 8);       
        gen.Emit(OpCodes.Ldarg_S, 9);       
        gen.Emit(OpCodes.Ldarg_S, 10);       
        gen.Emit(OpCodes.Call, baseUpdateMethod);
        gen.Emit(OpCodes.Ret);
        return (Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)dm.CreateDelegate(typeof(Action<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>));
    }
}