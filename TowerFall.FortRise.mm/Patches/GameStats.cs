using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;

/*
    Sidenote when modifying this patch, since this contains a bunch of Steam related stuff,
    I want anyone that touching this will ever expose any Steamworks related API. This
    to ensure that the compatibility remains the same across platforms and supported version.
*/

namespace TowerFall.Patching 
{
    [MonoModPatch("TowerFall.GameStats")]
    [MonoModIfFlag("NoLauncher")]
    public class GameStats : TowerFall.GameStats
    {
        // TODO: replace all field access with property calls for all instances of these:
        public int ArrowsShot { get; set; }
        public int TimesLaunched { get; set; }

        public int TotalVersusKills { get; set; }
        public int MatchesPlayed { get; set; }
        public int RoundsPlayed { get; set; }
        public int ArrowsCollected { get; set; }
        public int ArrowsCaught { get; set; }
        public int TreasuresTaken { get; set; }
        public int Dodges { get; set; }
        public int Jumps { get; set; }
        public int VersusRandomPlays { get; set; }
    }
}

namespace MonoMod 
{
    internal static partial class MonoModRules
    {
        private static void ReplaceFieldToProp(
            MethodDefinition method,
            string propDeclaringTypeName, 
            string targetField, 
            string propName)
        {
            if (!method.HasBody)
            {
                return;
            }
            var il = method.Body.GetILProcessor();
            var module = method.Module;

            TypeDefinition typeDef = module.GetType(propDeclaringTypeName);
            typeDef ??= module.Types.FirstOrDefault(t => t.FullName == propDeclaringTypeName);

            if (typeDef == null)
            {
                throw new InvalidOperationException($"[MonoMod] Could not resolve type '{propDeclaringTypeName}'");
            }

            MethodDefinition getMethod = null;
            foreach (var fMethod in typeDef.Methods)
            {
                if (fMethod.Name == "get_" + propName)
                {
                    getMethod = fMethod;
                    break;
                }
            }

            MethodDefinition setMethod = null;
            foreach (var fMethod in typeDef.Methods)
            {
                if (fMethod.Name == "set_" + propName)
                {
                    setMethod = fMethod;
                    break;
                }
            }

            for (int i = 0; i < method.Body.Instructions.Count; i += 1)
            {
                var instr = method.Body.Instructions[i];

                if (instr.Operand is not FieldReference fieldRef || fieldRef.Name != targetField)
                {
                    continue;
                }

                if (!propDeclaringTypeName.Contains(fieldRef.DeclaringType.Name))
                {
                    continue;
                }

                if (instr.OpCode == OpCodes.Ldfld || instr.OpCode == OpCodes.Ldsfld || instr.OpCode == OpCodes.Ldflda || instr.OpCode == OpCodes.Ldsflda)
                {
                    if ((instr.OpCode == OpCodes.Ldfld || instr.OpCode == OpCodes.Ldflda) && !getMethod.HasThis)
                    {
                        il.InsertBefore(instr, Instruction.Create(OpCodes.Pop));
                    }
                    instr.OpCode = OpCodes.Call;
                    instr.Operand = getMethod;
                }
                else if (instr.OpCode == OpCodes.Stfld || instr.OpCode == OpCodes.Stsfld)
                {
                    instr.OpCode = OpCodes.Call;
                    instr.Operand = setMethod;
                }
            }
        }

        public static void FixGameStatsArrowsShot(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "ArrowsShot", "ArrowsShot");
        }

        public static void FixGameStatsTimesLaunched(MethodDefinition method, CustomAttribute attrib)
        {
            if (isV1331)
            {
                ReplaceFieldToProp(method, "TowerFall.GameStats", "TimesLaunched", "TimesLaunched");
            }
        }

        public static void FixGameStatsTotalVersusKills(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "TotalVersusKills", "TotalVersusKills");
        }

        public static void FixGameStatsMatchesPlayed(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "MatchesPlayed", "MatchesPlayed");
        }

        public static void FixGameStatsRoundsPlayed(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "RoundsPlayed", "RoundsPlayed");
        }

        public static void FixGameStatsArrowsCollected(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "ArrowsCollected", "ArrowsCollected");
        }

        public static void FixGameStatsArrowsCaught(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "ArrowsCaught", "ArrowsCaught");
        }

        public static void FixGameStatsTreasuresTaken(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "TreasuresTaken", "TreasuresTaken");
        }

        public static void FixGameStatsDodges(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "Dodges", "Dodges");
        }


        public static void FixGameStatsJumps(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "Jumps", "Jumps");
        }

        public static void FixGameStatsVersusRandomPlays(MethodDefinition method, CustomAttribute attrib)
        {
            ReplaceFieldToProp(method, "TowerFall.GameStats", "VersusRandomPlays", "VersusRandomPlays");
        }
    }

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsArrowsShot))]
    internal class FixGameStatsArrowsShotAttribute : Attribute;

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsTimesLaunched))]
    internal class FixGameStatsTimesLaunchedAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsTotalVersusKills))]
    internal class FixGameStatsTotalVersusKillsAttribute : Attribute;

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsMatchesPlayed))]
    internal class FixGameStatsMatchesPlayedAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsRoundsPlayed))]
    internal class FixGameStatsRoundsPlayedAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsArrowsCollected))]
    internal class FixGameStatsArrowsCollectedAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsArrowsCaught))]
    internal class FixGameStatsArrowsCaughtAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsTreasuresTaken))]
    internal class FixGameStatsTreasuresTakenAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsDodges))]
    internal class FixGameStatsDodgesAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsJumps))]
    internal class FixGameStatsJumpsAttribute : Attribute; 

    [MonoModCustomMethodAttribute(nameof(MonoModRules.FixGameStatsVersusRandomPlays))]
    internal class FixGameStatsVersusRandomPlaysAttribute : Attribute; 
}
