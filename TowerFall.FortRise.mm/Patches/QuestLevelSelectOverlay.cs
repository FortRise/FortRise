using System;
using FortRise;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{

    public class patch_QuestLevelSelectOverlay : QuestLevelSelectOverlay
    {
        private int statsID;
        private MapScene map;
        private string levelDeathsString;
        private string levelAttemptsString;
        private string levelTimeString;
        private bool levelRed;
        private bool levelGold;

        public patch_QuestLevelSelectOverlay(MapScene map) : base(map)
        {
        }

        [MonoModIgnore]
        [MonoModConstructor]
        [PatchQuestLevelSelectOverlayCtor]
        public extern void ctor(MapScene map);

        public extern void orig_Update();

        public override void Update()
        {
            if (map.Selection is QuestMapButton)
                orig_Update();
            else
                base_Update();
        }

        [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
        [MonoModIgnore]
        public void base_Update() 
        {
            base.Update();
        }

        [MonoModReplace]
        private void RefreshLevelStats()
        {
            var levelSet = map.GetLevelSet();
            QuestTowerStats questTowerStats;
            if (levelSet == "TowerFall")
            {
                questTowerStats = SaveData.Instance.Quest.Towers[statsID];
            }
            else
            {
                questTowerStats = FortRiseModule.SaveData.AdventureQuest.AddOrGet(TowerRegistry.QuestGet(levelSet, statsID).GetLevelID());
            }


            if (questTowerStats.TotalDeaths == 0UL)
            {
                levelDeathsString = "";
            }
            else
            {
                levelDeathsString = questTowerStats.TotalDeaths.ToString();
            }
            if (questTowerStats.TotalAttempts == 0UL)
            {
                levelAttemptsString = "";
            }
            else
            {
                levelAttemptsString = questTowerStats.TotalAttempts.ToString();
            }
            long num;
            if (TFGame.PlayerAmount == 1)
            {
                num = questTowerStats.Best1PTime;
            }
            else
            {
                num = questTowerStats.Best2PTime;
            }
            if (num == 0L)
            {
                levelTimeString = "";
            }
            else
            {
                levelTimeString = TrialsResults.GetTimeString(num);
            }
            levelRed = questTowerStats.CompletedHardcore;
            levelGold = questTowerStats.CompletedNoDeaths;
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchQuestLevelSelectOverlayCtor))]
    internal class PatchQuestLevelSelectOverlayCtor : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchQuestLevelSelectOverlayCtor(ILContext ctx, CustomAttribute attrib) 
        {
            var TowerFall_MapScene = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapScene");
            var Selection = TowerFall_MapScene.FindField("Selection");

            var TowerFall_MapButton = ctx.Module.Assembly.MainModule.GetType("TowerFall", "MapButton");
            var get_Data = TowerFall_MapButton.FindMethod("TowerFall.TowerMapData get_Data()", false);

            var cursor = new ILCursor(ctx);
            var label = ctx.DefineLabel();
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.QuestLevelSelectOverlay", "drawStatsLerp"));

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldfld, Selection);
            cursor.Emit(OpCodes.Callvirt, get_Data);

            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld("TowerFall.QuestLevelSelectOverlay", "statsID"));
            cursor.MarkLabel(label);
            cursor.GotoPrev(MoveType.After, instr => instr.MatchCallvirt(get_Data));
            cursor.Emit(OpCodes.Brfalse_S, label);
        }
    }
}