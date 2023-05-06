using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.InlineRT;
using MonoMod.Utils;

namespace MonoMod;

internal class PostPatchDisableTempVariant : Attribute {}
internal class PostPatchEnableTempVariant : Attribute {}
internal class PostPatchXmlToVariant : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMapSceneBegin))]
internal class PatchMapSceneBegin : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldRoundLogicOnPlayerDeath))]
internal class PatchDarkWorldRoundLogicOnPlayerDeath : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldLevelSelectOverlayCtor))]
internal class PatchDarkWorldLevelSelectOverlayCtor : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldCompleteSequence))]
internal class PatchDarkWorldCompleteSequence : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldControlLevelSequence))]
internal class PatchDarkWorldControlLevelSequence : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestSpawnPortalFinishSpawn))]
internal class PatchQuestSpawnPortalFinishSpawn : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchSessionStartGame))]
internal class PatchSessionStartGame : Attribute {}

[MonoModCustomMethodAttribute(nameof(MonoModRules.PatchMainMenuCreateOptions))]
internal class PatchMainMenuCreateOptions : Attribute {}


internal static partial class MonoModRules 
{
    private static bool IsTowerFall;
    private static Version Version;

    static MonoModRules() 
    {
        MonoModRule.Modder.MissingDependencyThrow = false;
        bool hasSteamworks = false;
        foreach (var name in MonoModRule.Modder.Module.AssemblyReferences) 
        {
            if (name.Name.Contains("Steamworks"))
                hasSteamworks = true;
        }
        MonoModRule.Flag.Set("Steamworks", hasSteamworks);
        MonoModRule.Flag.Set("NoLauncher", !hasSteamworks);
        if (hasSteamworks) 
            Console.WriteLine("[FortRise] Running on a Steam Launcher");
        else 
            Console.WriteLine("[FortRise] Running TowerFall without a Launcher");
        

        TypeDefinition t_TFGame = MonoModRule.Modder.FindType("TowerFall.TFGame")?.Resolve();
        if (t_TFGame == null)
            return;
        IsTowerFall = t_TFGame.Scope == MonoModRule.Modder.Module;
        MonoModRule.Modder.PostProcessors += PostProcessor;

        // Get the version of TowerFall

        int[] numVersions = null;
        var ctor_TFGame = t_TFGame.FindMethod(".cctor", true);
        if (ctor_TFGame != null && ctor_TFGame.HasBody) 
        {
            var instrs = ctor_TFGame.Body.Instructions;
            for (int i = 0; i < instrs.Count; i++) 
            {
                var instr = instrs[i];
                var ctor_Version = instr.Operand as MethodReference;
                if (instr.OpCode != OpCodes.Newobj || ctor_Version.DeclaringType?.FullName != "System.Version")
                    continue;
                
                numVersions = new int[ctor_Version.Parameters.Count];
                for (int j = -numVersions.Length; j < 0; j++) 
                    numVersions[j + numVersions.Length] = instrs[j + i].GetInt();
                
                break;
            }
        }

        if (numVersions == null) {
            throw new InvalidOperationException("Unknown version of TowerFall is being patched. Operation cancelled");
        }

        var version = numVersions.Length switch {
            2 => new Version(numVersions[0], numVersions[1]),
            3 => new Version(numVersions[0], numVersions[1], numVersions[2]),
            4 => new Version(numVersions[0], numVersions[1], numVersions[2], numVersions[3]),
            _ => throw new InvalidOperationException("Unknown version of TowerFall is being patched. Operation cancelled")
        };
        var minimumVersion = new Version(1, 3, 3, 1);
        if (version.Major == 0)
            version = minimumVersion;
        if (version < minimumVersion)
            throw new Exception($"Unsupported version of TowerFall: {version}, currently supported: {minimumVersion}");
        Version = version;
        Console.WriteLine("[FortRise] TowerFall Version is: " + Version);
        
        if (IsTowerFall) 
        {
            // Ensure that TowerFall assembly is not already modded
            // (https://github.com/MonoMod/MonoMod#how-can-i-check-if-my-assembly-has-been-modded)
            if (MonoModRule.Modder.FindType("MonoMod.WasHere") != null)
                throw new Exception("This version of Celeste is already modded. You need a clean install of Celeste to mod it.");

            // Ensure that Microsoft.Xna.Framework.dll is present.
            if (MonoModRule.Modder.FindType("Microsoft.Xna.Framework.Game")?.SafeResolve() == null)
                throw new Exception("MonoModRules failed resolving Microsoft.Xna.Framework.Game");
        }

        var isWindows = PlatformHelper.Is(Platform.Windows);
        MonoModRule.Flag.Set("OS:Windows", isWindows);
        MonoModRule.Flag.Set("OS:NotWindows", !isWindows);
        Console.WriteLine($"[FortRise] Platform Found: {PlatformHelper.Current}");
    }

    public static void PatchDarkWorldRoundLogicOnPlayerDeath(ILContext ctx, CustomAttribute attrib) 
    {
        var SaveData = ctx.Module.Assembly.MainModule.GetType("TowerFall", "SaveData");
        var AdventureActive = SaveData.FindField("AdventureActive");
        var cursor = new ILCursor(ctx);
        var label = ctx.DefineLabel();

        cursor.GotoNext(
            MoveType.After,
            instr => instr.MatchAdd(),
            instr => instr.MatchStfld("TowerFall.DarkWorldTowerStats", "Deaths")
        );
        cursor.MarkLabel(label);

        cursor.GotoPrev(instr => instr.MatchCallOrCallvirt("TowerFall.RoundLogic", "OnPlayerDeath"));
        cursor.GotoNext();
        cursor.Emit(OpCodes.Ldsfld, AdventureActive);
        cursor.Emit(OpCodes.Brtrue_S, label);
    }

    public static void PatchSessionStartGame(ILContext ctx, CustomAttribute attrib) 
    {
        var SaveData = ctx.Module.Assembly.MainModule.GetType("TowerFall", "SaveData");
        var AdventureActive = SaveData.FindField("AdventureActive");
        var cursor = new ILCursor(ctx);

        cursor.GotoNext(
            MoveType.After,
            instr => instr.MatchAdd(),
            instr => instr.MatchStfld("TowerFall.DarkWorldTowerStats", "Attempts")
        );
        var label = ctx.DefineLabel(cursor.Next);

        cursor.GotoPrev(MoveType.After, instr => instr.MatchStfld("TowerFall.Session", "DarkWorldState"));
        cursor.Emit(OpCodes.Ldsfld, AdventureActive);
        cursor.Emit(OpCodes.Brtrue_S, label);
    }

    public static void PatchMainMenuCreateOptions(ILContext ctx, CustomAttribute attrib) 
    {
        var riseCore = ctx.Module.GetType("FortRise.RiseCore");
        var invoked = riseCore.FindMethod("System.Void InvokeMainMenu_CreateOptions(System.Collections.Generic.List`1<TowerFall.OptionsButton>)");
        var cursor = new ILCursor(ctx);
        cursor.GotoNext(MoveType.Before, 
            instr => instr.MatchLdloc(0), 
            instr => instr.MatchLdstr("CLEAR ALL GAME DATA")
        );
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.Emit(OpCodes.Call, invoked);
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
            var riseCore = ctx.Module.GetType("FortRise.RiseCore");
            var invoked = riseCore.FindMethod("System.Void InvokeDarkWorldComplete_Result(System.Int32,TowerFall.DarkWorldDifficulties,System.Int32,System.Int64,System.Int32,System.Int32,System.Int32)");
            var SaveData = ctx.Module.GetType("TowerFall", "SaveData");
            var AdventureActive = SaveData.FindField("AdventureActive");
            var deaths = complete.DeclaringType.FindField("<deaths>5__2");
            var this_4 = complete.DeclaringType.FindField("<>4__this");

            var session = method.DeclaringType.FindField("session");
            var matchSettings = session.FieldType.Resolve().FindField("MatchSettings");

            var darkWorldState = session.FieldType.Resolve().FindField("DarkWorldState");
            var time = darkWorldState.FieldType.Resolve().FindField("Time");
            var continues = darkWorldState.FieldType.Resolve().FindField("Continues");

            var Variants = matchSettings.FieldType.Resolve().FindField("Variants");
            var GetCoopCurses = Variants.FieldType.Resolve().FindMethod("System.Int32 GetCoOpCurses()");
            var DarkWorldDifficulty = matchSettings.FieldType.Resolve().FindField("DarkWorldDifficulty");
            var TFGame_PlayerAmount = ctx.Module.GetType("TowerFall.TFGame").FindMethod("System.Int32 get_PlayerAmount()");

            var levelSystem = matchSettings.FieldType.Resolve().FindField("LevelSystem");
            var get_ID = levelSystem.FieldType.Resolve().FindMethod("Microsoft.Xna.Framework.Point get_ID()");
            var X = ctx.Module.ImportReference(get_ID.ReturnType.Resolve().FindField("X"));

            var loc_matchSettings = new VariableDefinition(matchSettings.FieldType);
            var loc_darkworldstate = new VariableDefinition(ctx.Module.GetType("TowerFall.DarkWorldSessionState"));
            ctx.Body.Variables.Add(loc_matchSettings);
            ctx.Body.Variables.Add(loc_darkworldstate);

            var cursor = new ILCursor(ctx);

            cursor.GotoNext(instr => instr.MatchLdsfld("TowerFall.SaveData", "Instance"));
            // This part of instructions will replace one method call from the DarkWorldTowerStats into a hook
            
            // Check for TF Version since it does have a different instructions
            // Please confirm if you have an issue with 1.3.3.2, I will fix this later on

            var instrNumToRemove = Version switch {
                { Major: 1, Minor: 3, Build: 3, Revision: 3} => 31,
                _ => 36
            };
            cursor.RemoveRange(instrNumToRemove);

            /* matchSettings */
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, this_4);
            cursor.Emit(OpCodes.Ldfld, session);
            cursor.Emit(OpCodes.Ldfld, matchSettings);
            cursor.Emit(OpCodes.Stloc_S, loc_matchSettings);

            /* darkWorldState */
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, this_4);
            cursor.Emit(OpCodes.Ldfld, session);
            cursor.Emit(OpCodes.Ldfld, darkWorldState);
            cursor.Emit(OpCodes.Stloc_S, loc_darkworldstate);

            /* Emit necessary code to call the InvokeDarkWorldComplete_Result hook */
            cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
            cursor.Emit(OpCodes.Ldfld, levelSystem);
            cursor.Emit(OpCodes.Callvirt, get_ID);
            cursor.Emit(OpCodes.Ldfld, X);
            cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
            cursor.Emit(OpCodes.Ldfld, DarkWorldDifficulty);
            cursor.Emit(OpCodes.Call, TFGame_PlayerAmount);
            cursor.Emit(OpCodes.Ldloc_S, loc_darkworldstate);
            cursor.Emit(OpCodes.Ldfld, time);
            cursor.Emit(OpCodes.Ldloc_S, loc_darkworldstate);
            cursor.Emit(OpCodes.Ldfld, continues);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, deaths);
            cursor.Emit(OpCodes.Ldloc_S, loc_matchSettings);
            cursor.Emit(OpCodes.Ldfld, Variants);
            cursor.Emit(OpCodes.Callvirt, GetCoopCurses);
            cursor.Emit(OpCodes.Call, invoked);
        });
    }

    public static void PatchQuestSpawnPortalFinishSpawn(ILContext ctx, CustomAttribute attrib) 
    {
        var LevelEntity = ctx.Module.GetType("TowerFall.LevelEntity");
        var get_level = LevelEntity.FindMethod("TowerFall.Level get_Level()");
        var Entity = ctx.Module.GetType("Monocle.Entity");
        var Position = Entity.FindField("Position");
        var RiseCore = ctx.Module.GetType("FortRise.RiseCore");
        var InvokeEvent = RiseCore.FindMethod(
            "System.Void InvokeQuestSpawnPortal_FinishSpawn(System.String,Microsoft.Xna.Framework.Vector2,TowerFall.Facing,TowerFall.Level)");
        var cursor = new ILCursor(ctx);
        cursor.GotoNext(instr => instr.MatchLdstr("Unknown enemy type: "));
        // cursor.GotoNext();

        cursor.RemoveRange(5);
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldfld, Position);
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Call, get_level);
        cursor.Emit(OpCodes.Call, InvokeEvent);
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
        var matchVariant = modder.Module.Types.Where(x => x.FullName == "TowerFall.MatchVariants").First();
        foreach (TypeDefinition type in modder.Module.Types) 
        {
            if (type.FullName == "TowerFall.TemporaryVariants") 
            {
                foreach (var field in matchVariant.Fields) 
                {
                    if (!field.FieldType.Is("TowerFall.Variant"))
                        continue;
                    FieldDefinition def = new FieldDefinition(
                        "Temp" + field.Name, FieldAttributes.Public | FieldAttributes.Static, 
                        modder.Module.TypeSystem.Boolean 
                    );
                    type.Fields.Add(def);
                }
            }
            else if (type.FullName == "TowerFall.DarkWorldTowerData") 
            {
                var levelData = type.NestedTypes.Where(x => x.FullName == "TowerFall.DarkWorldTowerData/LevelData").First();
                var variantField = levelData.FindField("ActiveVariant");
                var variant = variantField.FieldType.Resolve();
                var MatchVariants = modder.Module.GetType("TowerFall.MatchVariants");
                foreach (var field in MatchVariants.Fields) 
                {
                    if (!field.FieldType.Is("TowerFall.Variant"))
                        continue;
                    var newField = new FieldDefinition(field.Name, FieldAttributes.Public, modder.Module.TypeSystem.Boolean);
                    variant.Fields.Add(newField);
                }
                foreach (var methd in levelData.Methods) 
                {
                    if (!methd.HasCustomAttribute("MonoMod.PostPatchXmlToVariant"))
                        continue;

                    var Calc = modder.Module.GetType("Monocle.Calc");
                    var HasChild = Calc.FindMethod("System.Boolean HasChild(System.Xml.XmlElement,System.String)");
                    var ChildBool = Calc.FindMethod("System.Boolean ChildBool(System.Xml.XmlElement,System.String)");

                    var il = methd.Body.GetILProcessor();
                    il.RemoveAt(0);

                    for (int i = 0; i < variant.Fields.Count; i++) 
                    {
                        var field = variant.Fields[i];
                        var fieldName = variant.FindField(field.Name);
                        var entry = i == variant.Fields.Count - 1 ? 
                            il.Create(OpCodes.Ret) : il.Create(OpCodes.Ldarg_1);
                        
                        if (i == 0) 
                            il.Emit(OpCodes.Ldarg_1);
                        
                        il.Emit(OpCodes.Ldstr, field.Name);
                        il.Emit(OpCodes.Call, HasChild);
                        il.Emit(OpCodes.Brfalse_S, entry);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, variantField);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldstr, field.Name);
                        il.Emit(OpCodes.Call, ChildBool);
                        il.Emit(OpCodes.Stfld, fieldName);
                        il.Append(entry);
                    }
                }
            }
            PostProcessType(modder, type);
        }

        var controlType = modder.Module.GetType("TowerFall.DarkWorldControl");

        foreach (var methd in controlType.Methods) 
        {
            var tempVariant = modder.Module.GetType("TowerFall.TemporaryVariants");
            // PostPatch
            if (methd.HasCustomAttribute("MonoMod.PostPatchDisableTempVariant")) 
            {
                var TempDark = tempVariant.FindField("TempAlwaysDark");
                var Level = modder.Module.GetType("TowerFall.Level");
                var get_Session = Level.FindMethod("TowerFall.Session get_Session()");

                var Session = modder.Module.GetType("TowerFall.Session");
                var MatchSettings = Session.FindField("MatchSettings");
                var Variants = MatchSettings.FieldType.Resolve().FindField("Variants");

                
                var il = methd.Body.GetILProcessor();
                il.RemoveAt(0);
                for (int i = 0; i < tempVariant.Fields.Count; i++) 
                {
                    var field = tempVariant.Fields[i];
                    var variant = Variants.FieldType.Resolve().FindField(field.Name.Remove(0, 4));
                    var set_Value = variant.FieldType.Resolve().FindMethod("System.Void set_Value(System.Boolean)");
                    // if (TempVariant)
                    if (i == 0)
                        il.Emit(OpCodes.Ldsfld, field);
                    var entry = i == tempVariant.Fields.Count - 1 ? 
                        il.Create(OpCodes.Ret) : il.Create(OpCodes.Ldsfld, tempVariant.Fields[i + 1]);
                    il.Emit(OpCodes.Brfalse_S, entry);
                    // Body
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Stsfld, field);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, get_Session);
                    il.Emit(OpCodes.Ldfld, MatchSettings);
                    il.Emit(OpCodes.Ldfld, Variants);
                    il.Emit(OpCodes.Ldfld, variant);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Callvirt, set_Value);
                    il.Append(entry);
                }
            }
            else if (methd.HasCustomAttribute("MonoMod.PostPatchEnableTempVariant"))  
            {
                var Level = modder.Module.GetType("TowerFall.Level");
                var get_Session = Level.FindMethod("TowerFall.Session get_Session()");

                var Session = modder.Module.GetType("TowerFall.Session");
                var TowerData = modder.Module.GetType("TowerFall.DarkWorldTowerData/LevelData");
                var variantField = TowerData.FindField("ActiveVariant");
                var MatchSettings = Session.FindField("MatchSettings");
                var Variants = MatchSettings.FieldType.Resolve().FindField("Variants");

                var il = methd.Body.GetILProcessor();
                il.RemoveAt(0);

                for (int i = 0; i < tempVariant.Fields.Count; i++)  
                {
                    var field = tempVariant.Fields[i];
                    var fieldVariant = field.Name.Remove(0, 4);
                    var towerVariant = variantField.FieldType.Resolve().FindField(fieldVariant);
                    var variant = Variants.FieldType.Resolve().FindField(fieldVariant);
                    var get_Value = variant.FieldType.Resolve().FindMethod("System.Boolean get_Value()");
                    var set_Value = variant.FieldType.Resolve().FindMethod("System.Void set_Value(System.Boolean)");

                    var entry = i == tempVariant.Fields.Count - 1 ? 
                        il.Create(OpCodes.Ret) : il.Create(OpCodes.Ldarg_0);
                    if (i == 0)
                        il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, get_Session);
                    il.Emit(OpCodes.Ldfld, MatchSettings);
                    il.Emit(OpCodes.Ldfld, Variants);
                    il.Emit(OpCodes.Ldfld, variant);
                    il.Emit(OpCodes.Callvirt, get_Value);
                    il.Emit(OpCodes.Brtrue_S, entry);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldflda, variantField);
                    il.Emit(OpCodes.Ldfld, towerVariant);
                    il.Emit(OpCodes.Brfalse_S, entry);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Stsfld, field);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, get_Session);
                    il.Emit(OpCodes.Ldfld, MatchSettings);
                    il.Emit(OpCodes.Ldfld, Variants);
                    il.Emit(OpCodes.Ldfld, variant);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Callvirt, set_Value);
                    il.Append(entry);
                }
            }
        }
    }

    // https://github.com/EverestAPI/Everest/blob/f4545220fe22ed3f752e358741befe9cc7546234/Celeste.Mod.mm/MonoModRules.cs
    // This is to fix the Enumerators can't be decompiled
    public static void FixEnumeratorDecompile(TypeDefinition type) {
    foreach (MethodDefinition method in type.Methods) {
        new ILContext(method).Invoke(il => {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchCallvirt(out MethodReference m) &&
                (m.Name is "System.Collections.IEnumerable.GetEnumerator" or "System.IDisposable.Dispose" ||
                    m.Name.StartsWith("<>m__Finally")))
            ) {
                cursor.Next.OpCode = OpCodes.Call;
            }
        });
    }
}

    private static void PostProcessType(MonoModder modder, TypeDefinition type) 
    {
        if (type.IsCompilerGeneratedEnumerator()) {
            FixEnumeratorDecompile(type);
        }
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