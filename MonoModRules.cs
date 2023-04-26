using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.InlineRT;
using MonoMod.Utils;

namespace MonoMod;

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMapSceneBegin))]
internal class PatchMapSceneBegin : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchOnPlayerDeath))]
internal class PatchOnPlayerDeath : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldLevelSelectOverlayCtor))]
internal class PatchDarkWorldLevelSelectOverlayCtor : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldCompleteSequence))]
internal class PatchDarkWorldCompleteSequence : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldControlLevelSequence))]
internal class PatchDarkWorldControlLevelSequence : Attribute {}


internal static partial class MonoModRules 
{
    private static bool IsTowerFall;
    private static TypeDefinition TowerFall;

    static MonoModRules() 
    {
        MonoModRule.Modder.MissingDependencyThrow = false;
        MonoModRule.Modder.PostProcessors += PostProcessor;
    }


    public static void PatchOnPlayerDeath(ILContext ctx, CustomAttribute attrib) 
    {
        var SaveData = ctx.Module.Assembly.MainModule.GetType("TowerFall", "SaveData");
        var AdventureActive = SaveData.FindField("AdventureActive");
        var cursor = new ILCursor(ctx);

        var label = ctx.DefineLabel();

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdarg(3));
        cursor.GotoNext();
        cursor.GotoNext();
        cursor.GotoNext();
        
        cursor.Emit(OpCodes.Ldsfld, AdventureActive);

        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdarg(0));
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdarg(0));
        cursor.MarkLabel(label);
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdarg(3));
        cursor.GotoNext();
        cursor.GotoNext();
        cursor.GotoNext();
        cursor.Emit(OpCodes.Brfalse_S, label);
    }

    public static void PatchDarkWorldControlLevelSequence(MethodDefinition method, CustomAttribute attrib) 
    {
        MethodDefinition complete = method.GetEnumeratorMoveNext();

        FieldDefinition f_levelData = complete.DeclaringType.Fields.FirstOrDefault(
            f => f.Name.StartsWith("<levelData>5__1")
        );

        new ILContext(complete).Invoke(ctx => {
            var TowerFall_DarkWorldTowerData = ctx.Module.Assembly.MainModule.GetType("TowerFall.DarkWorldTowerData");
            var LevelData = TowerFall_DarkWorldTowerData.NestedTypes.FirstOrDefault(t => t.Name.StartsWith("LevelData"));
            var Dark = LevelData.FindField("Dark");
            var cursor = new ILCursor(ctx);
            var labelstart = ctx.DefineLabel();

            cursor.GotoNext(MoveType.After,
                instr => instr.MatchLdfld("TowerFall.MatchVariants", "AlwaysDark"),
                instr => instr.MatchCallvirt("TowerFall.Variant", "get_Value"));
            
            cursor.GotoNext(instr => instr.MatchLdloc(1));
            cursor.MarkLabel(labelstart);
            cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt("TowerFall.Variant", "get_Value"));
            
            cursor.Emit(OpCodes.Brtrue_S, labelstart);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, f_levelData);
            cursor.Emit(OpCodes.Ldfld, Dark);

            var labelend = ctx.DefineLabel();

            cursor.GotoNext(MoveType.After,
                instr => instr.MatchLdfld("TowerFall.MatchVariants", "AlwaysDark"),
                instr => instr.MatchCallvirt("TowerFall.Variant", "get_Value"));
            
            cursor.GotoNext(instr => instr.MatchLdarg(0));
            cursor.MarkLabel(labelend);
            cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt("TowerFall.Variant", "get_Value"));
            
            cursor.Emit(OpCodes.Brtrue_S, labelend);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, f_levelData);
            cursor.Emit(OpCodes.Ldfld, Dark);
        });
    }

    public static void PatchDarkWorldCompleteSequence(MethodDefinition method, CustomAttribute attribute) 
    {
        MethodDefinition complete = method.GetEnumeratorMoveNext();

        new ILContext(complete).Invoke(ctx => {
            var SaveData = ctx.Module.Assembly.MainModule.GetType("TowerFall", "SaveData");
            var AdventureActive = SaveData.FindField("AdventureActive");
            var cursor = new ILCursor(ctx);
            var label = ctx.DefineLabel();

            cursor.GotoNext(instr => instr.MatchLdcI4(20));
            cursor.GotoNext(instr => instr.MatchLdcI4(20));
            cursor.GotoNext(instr => instr.MatchLdarg(0));
            cursor.GotoNext(instr => instr.MatchLdarg(0));
            cursor.GotoNext();
            cursor.GotoNext();
            cursor.GotoNext();

            cursor.Emit(OpCodes.Ldsfld, AdventureActive);

            cursor.GotoNext(MoveType.Before, 
                instr => instr.MatchLdarg(0));
            cursor.GotoNext(MoveType.Before, 
                instr => instr.MatchLdarg(0));
            cursor.MarkLabel(label);

            cursor.GotoPrev(MoveType.After, instr => instr.MatchLdsfld(AdventureActive));
            cursor.Emit(OpCodes.Brtrue_S, label);
        });
    }

    public static void PatchDarkWorldLevelSelectOverlayCtor(ILContext ctx, CustomAttribute attrib) 
    {
        var TowerFall_MapScene = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapScene");
        var Selection = TowerFall_MapScene.FindField("Selection");

        var TowerFall_MapButton = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapButton");
        var get_Data = TowerFall_MapButton.FindMethod("TowerFall.TowerMapData get_Data()", false);

        var cursor = new ILCursor(ctx);
        var label = ctx.DefineLabel();
        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.DarkWorldLevelSelectOverlay", "drawStatsLerp"));

        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldfld, Selection);
        cursor.Emit(OpCodes.Callvirt, get_Data);

        cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.DarkWorldLevelSelectOverlay", "statsID"));
        cursor.MarkLabel(label);
        cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt(get_Data));
        cursor.Emit(OpCodes.Brfalse_S, label);
    }

    public static void PatchMapSceneBegin(ILContext ctx, CustomAttribute attrib) 
    {
        var method = ctx.Method.DeclaringType.FindMethod("System.Void InitAdventureMap()");

        ILCursor cursor = new ILCursor(ctx);

        cursor.GotoNext(MoveType.After, instr => instr.MatchLdcI4(0));
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdcI4(0));
        cursor.GotoNext(MoveType.Before, instr => instr.MatchLdcI4(0));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, method);
    }

    public static void PostProcessor(MonoModder modder) 
    {
        foreach (TypeDefinition type in modder.Module.Types) 
        {
            PostProcessType(modder, type);
        }
    }

    private static void PostProcessType(MonoModder modder, TypeDefinition type) 
    {
        foreach (MethodDefinition method in type.Methods) 
        {
            method.FixShortLongOps();
        }
        foreach (TypeDefinition nested in type.NestedTypes) 
        {
            PostProcessType(modder, nested);
        }
    }
}