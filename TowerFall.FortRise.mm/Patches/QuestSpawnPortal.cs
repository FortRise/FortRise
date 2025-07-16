using System;
using System.Collections.Generic;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public partial class patch_QuestSpawnPortal : QuestSpawnPortal
    {
        public patch_QuestSpawnPortal(Vector2 position, Vector2[] nodes) : base(position, nodes)
        {
        }


        [MonoModIgnore]
        private extern bool Disappear();

        [PatchQuestSpawnPortalFinishSpawn]
        [MonoModIgnore]
        private extern void FinishSpawn(Sprite<int> sprite);

        public void SpawnExtraEntity(string name, Facing facing)
        {
            Vector2 position = Position;
            Vector2[] nodes = Nodes;
            if (RiseCore.EnemyLoader.TryGetValue(name, out EnemyLoader loader)) 
            {
                Level.Add(loader?.Invoke(position + new Vector2(0f, 2f), facing, nodes));
                return;
            }
            if (name.Contains("Skeleton") || name.Contains("Jester")) 
            {
                bool hasShields = false;
                bool hasWings = false;
                bool canMimic = false;
                bool jester = false;
                bool boss = false;

                if (name.EndsWith("S"))
                    hasShields = true;

                if (name.Contains("Wing"))
                    hasWings = true;

                if (name.Contains("Mimic"))
                    canMimic = true;

                if (name.Contains("Boss"))
                    boss = true;
                
                if (name.Contains("Jester"))
                    jester = true;
                
                ArrowTypes arrows = GetSkeletonArrowTypes(name);
                Level.Add(new Skeleton(position + new Vector2(0f, 2f), facing, arrows, hasShields, hasWings, canMimic, jester, boss));
                return;
            }

            Logger.Error($"Entity name: {name} failed to spawn as it does not exists!");
            Sounds.ui_levelLock.Play(160f);
            Level.ScreenShake(8);

        }

        public static ArrowTypes GetSkeletonArrowTypes(string name) 
        {
            var colonIndex = name.IndexOf(':');
            if (colonIndex != -1) 
            {
                var arrowName = name.Substring(colonIndex + 1);
                if (ArrowsRegistry.StringToTypes.TryGetValue(arrowName, out var type)) 
                {
                    return type;
                }
                Logger.Error($"[Skeleton Arrow] Arrow Name: '{arrowName}' not found!");
            }
            
            if (name.Contains("SuperBomb"))
                return ArrowTypes.SuperBomb;
            if (name.Contains("Bomb"))
                return ArrowTypes.Bomb;
            if (name.Contains("Bramble"))
                return ArrowTypes.Bramble;
            if (name.Contains("Drill"))
                return ArrowTypes.Drill;
            if (name.Contains("Trigger"))
                return ArrowTypes.Trigger;
            if (name.Contains("Bolt"))
                return ArrowTypes.Bolt;
            if (name.Contains("Toy"))
                return ArrowTypes.Toy;
            if (name.Contains("Feather"))
                return ArrowTypes.Feather;
            if (name.Contains("Laser"))
                return ArrowTypes.Laser;
            if (name.Contains("Prism"))
                return ArrowTypes.Prism;
            if (name.Contains("VanillaRandom"))
                return GetRandomArrowType(true);
            if (name.Contains("Random"))
                return GetRandomArrowType(false);
            return ArrowTypes.Normal;
        }

        public static ArrowTypes GetRandomArrowType(bool vanilla)
        {
            List<ArrowTypes> list = new()
            {
                ArrowTypes.Normal,
                ArrowTypes.Bomb,
                ArrowTypes.Laser,
                ArrowTypes.Bramble,
                ArrowTypes.Drill,
                ArrowTypes.Bolt,
                ArrowTypes.SuperBomb,
                ArrowTypes.Feather
            };
            if (TowerFall.GameData.DarkWorldDLC)
            {
                list.Add(ArrowTypes.Trigger);
                list.Add(ArrowTypes.Prism);
            }
            if (!vanilla) 
            {
                foreach (var customArrow in ArrowsRegistry.GetArrowEntries().Values)
                {
                    list.Add(customArrow.ArrowTypes);
                }
            }

            
            if (list.Count == 0)
            {
                return ArrowTypes.Normal;
            }
            return Calc.Random.Choose(list);
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestSpawnPortalFinishSpawn))]
    internal class PatchQuestSpawnPortalFinishSpawn : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestSpawnPortalFinishSpawn(ILContext ctx, CustomAttribute attrib) 
        {
            var QuestSpawnPortal = ctx.Module.GetType("TowerFall.QuestSpawnPortal");
            var InvokeEvent = QuestSpawnPortal.FindMethod(
                "System.Void SpawnExtraEntity(System.String,TowerFall.Facing)");

            var label = ctx.DefineLabel();
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(instr => instr.MatchLdfld("TowerFall.QuestSpawnPortal", "addCounter"));
            cursor.GotoPrev();
            cursor.MarkLabel(label);
            cursor.Index = 0;

            cursor.GotoNext(MoveType.After, instr => instr.MatchLdstr("Unknown enemy type: "));
            cursor.Prev.OpCode = OpCodes.Ldarg_0;

            cursor.RemoveRange(4);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Call, InvokeEvent);
            cursor.Emit(OpCodes.Br, label);
        }
    }
}