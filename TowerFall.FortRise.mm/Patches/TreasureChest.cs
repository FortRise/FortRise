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
    public class patch_TreasureChest : TreasureChest
    {
        public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups pickups, int timer = 0) : base(position, graphic, mode, pickups, timer)
        {
        }

        public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups[] pickups, int timer = 0) : base(position, graphic, mode, pickups, timer)
        {
        }

        [PatchTreasureChestCtor_Arr]
        [MonoModConstructor]
        [MonoModIgnore]
        public extern void ctor(Vector2 position, Types graphic, AppearModes mode, Pickups[] pickups, int timer = 0);

        [PatchTreasureChestCtor]
        [MonoModConstructor]
        [MonoModIgnore]
        public extern void ctor(Vector2 position, Types graphic, AppearModes mode, Pickups pickups, int timer = 0);

        public static Pickups[] ChangePickup(Pickups[] pickups, Types types) 
        {
            var rand = Calc.Random.Range(0, 50);
            Logger.Log(rand);
            var arrowsList = new List<ArrowObject>(RiseCore.ArrowsRegistry.Values);
            arrowsList.Shuffle();
            foreach (var arrows in arrowsList) 
            {
                var arrowInfo = arrows.SpawnType;
                if (types.HasFlag(TreasureChest.Types.Normal) && arrowInfo.HasFlag(types) && rand < 30)
                {
                    Logger.Log(arrows.PickupType);
                    pickups[0] = arrows.PickupType;
                    return pickups;
                }
                    var i = Calc.Random.Range(0, 3);
                if (types.HasFlag(TreasureChest.Types.Large) && arrowInfo.HasFlag(types) && rand < 30)
                {
                    pickups[i] = arrows.PickupType;
                    return pickups;
                }
            }
            return pickups;
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchTreasureChestCtor))]
    public class PatchTreasureChestCtor : Attribute {}

    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchTreasureChestCtor_Arr))]
    public class PatchTreasureChestCtor_Arr : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchTreasureChestCtor_Arr(ILContext ctx, CustomAttribute attrib) 
        {
            var TreasureChest = ctx.Module.GetType("TowerFall.TreasureChest");
            var m_ChangePickup = TreasureChest.FindMethod("TowerFall.Pickups[] ChangePickup(TowerFall.Pickups[],TowerFall.TreasureChest/Types)");
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(instr => instr.MatchNewobj("System.Collections.Generic.List`1<TowerFall.Pickups>"));
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Call, m_ChangePickup);
        }

        public static void PatchTreasureChestCtor(ILContext ctx, CustomAttribute attrib) 
        {
            var TreasureChest = ctx.Module.GetType("TowerFall.TreasureChest");
            var m_ChangePickup = TreasureChest.FindMethod("TowerFall.Pickups[] ChangePickup(TowerFall.Pickups[],TowerFall.TreasureChest/Types)");
            var cursor = new ILCursor(ctx);
            cursor.GotoNext(MoveType.After, instr => instr.MatchStelemI4());
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.Emit(OpCodes.Call, m_ChangePickup);
        }
    }
}
