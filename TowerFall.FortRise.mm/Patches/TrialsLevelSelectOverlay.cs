using System;
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
    public class patch_TrialsLevelSelectOverlay : TrialsLevelSelectOverlay
    {
        private MapScene map;
        private Point statsID;
        private Sprite<int> levelMedalIcon;
        private Sprite<int> diamondIcon;
        private Sprite<int> goldIcon;
        private Sprite<int> devIcon;
        private string totalTimeString;
        private string totalGoldsString;
        private string totalDiamondsString;
        private string totalDevtimesString;
        private float drawStatsLerp;
        private string levelBestTimeString;
        private string levelAttemptsString;

        public patch_TrialsLevelSelectOverlay(MapScene map) : base(map)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchTrialsLevelSelectOverlayCtor]
        public extern void ctor(MapScene map);

        [Prefix(nameof(Update))]
        private bool Update_Prefix()
        {
            return map.Selection is TrialsMapButton;
        }

        private void RefreshLevelStats()
        {
            var tower = GameData.TrialsLevels[statsID.X, statsID.Y];
            long bestTime;
            bool unlockedDevTime;
            bool unlockedDiamond;
            bool unlockedGold;
            ulong attempts;
            if (tower.TowerSet == "TowerFall")
            {
                var trialsLevelStats = SaveData.Instance.Trials.Levels[statsID.X][statsID.Y];
                bestTime = trialsLevelStats.BestTime;
                unlockedDevTime = trialsLevelStats.UnlockedDevTime;
                unlockedDiamond = trialsLevelStats.UnlockedDiamond;
                unlockedGold = trialsLevelStats.UnlockedGold;
                attempts = trialsLevelStats.Attempts;
            }
            else
            {
                var trialsLevelStats = FortRiseModule.SaveData.AdventureTrials.AddOrGet(tower.LevelID);
                bestTime = trialsLevelStats.BestTime;
                unlockedDevTime = trialsLevelStats.UnlockedDevTime;
                unlockedDiamond = trialsLevelStats.UnlockedDiamond;
                unlockedGold = trialsLevelStats.UnlockedGold;
                attempts = trialsLevelStats.Attempts;
            }
            if (bestTime == 0L)
            {
                levelBestTimeString = "";
            }
            else
            {
                levelBestTimeString = TrialsResults.GetTimeString(bestTime);
                if (unlockedDevTime)
                {
                    levelMedalIcon = devIcon;
                }
                else if (unlockedDiamond)
                {
                    levelMedalIcon = diamondIcon;
                }
                else if (unlockedGold)
                {
                    levelMedalIcon = goldIcon;
                }
            }
            if (attempts == 0UL)
            {
                levelAttemptsString = "";
                return;
            }
            levelAttemptsString = attempts.ToString();
        }
    }
}



namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchTrialsLevelSelectOverlayCtor))]
    internal class PatchTrialsLevelSelectOverlayCtor : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchTrialsLevelSelectOverlayCtor(ILContext ctx, CustomAttribute attrib) 
        {
            var TowerFall_MapScene = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapScene");
            var Selection = TowerFall_MapScene.FindField("Selection");

            var TowerFall_MapButton = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapButton");
            var get_Data = TowerFall_MapButton.FindMethod("TowerFall.TowerMapData get_Data()", false);

            var cursor = new ILCursor(ctx);
            var label = ctx.DefineLabel();
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.TrialsLevelSelectOverlay", "drawStatsLerp"));

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldfld, Selection);
            cursor.Emit(OpCodes.Callvirt, get_Data);

            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.TrialsLevelSelectOverlay", "statsID"));
            cursor.MarkLabel(label);
            cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt(get_Data));
            cursor.Emit(OpCodes.Brfalse_S, label);
        }
    }
}
