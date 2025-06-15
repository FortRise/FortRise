using System;
using System.Collections;
using System.Collections.Generic;
using FortRise;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

namespace TowerFall 
{
    public class patch_DarkWorldControl : DarkWorldControl 
    {
        private List<QuestSpawnPortal> allPortals;
		private List<QuestSpawnPortal> groundPortals;
		private List<QuestSpawnPortal> airPortals;
		private List<QuestSpawnPortal> nodePortals;

        private List<Pickups> midwayTreasure;
        private bool bossMode;


        [PatchDarkWorldControlLevelSequence]
        private extern IEnumerator orig_LevelSequence();

        [MonoModReplace]
        private void StartMusic()
        {
            if (bossMode)
                return;
            
            var levelSession = Level.Session;
            var themeMusic = levelSession.MatchSettings.LevelSystem.Theme.Music;
            levelSession.SongTimer = 0;
            Music.Play(themeMusic);
        }

        private IEnumerator LevelSequence() 
        {
            var matchSettings = Level.Session.MatchSettings;

            if (matchSettings.Variants.SlowTime)
            {
                Level.OrbLogic.DoTimeOrb(delay: true);
            }
            if (matchSettings.Variants.AlwaysLava)
            {
                Level.OrbLogic.DoLavaVariant();
            }
            if (matchSettings.Variants.AlwaysScrolling)
            {
                Level.OrbLogic.StartScroll();
            }

            yield return orig_LevelSequence();
        }

        [MonoModReplace]
        private void DoBossLevelSetup(int bossID, List<Pickups> treasure) 
        {
            var levelData = (Level.Session.MatchSettings.LevelSystem as patch_DarkWorldLevelSystem)
                .GetLevelData(base.Level.Session.MatchSettings.DarkWorldDifficulty, base.Level.Session.RoundIndex);

			this.midwayTreasure = treasure;
			int darkWorldDifficulty = (int)base.Level.Session.MatchSettings.DarkWorldDifficulty;
            
            switch (bossID)
            {
                case 0:
                    Level.Add(new AmaranthBoss(darkWorldDifficulty));
                    break;
                case 1:
                    Level.Add(new DreadwoodBossControl(darkWorldDifficulty));
                    break;
                case 2:
                    Level.Add(new CyclopsEye(darkWorldDifficulty));
                    break;
                case 3:
                    Level.Add(new CataclysmEye(darkWorldDifficulty));
                    break;
                default:
                    Level.Add(DarkWorldBossRegistry.DarkWorldBossLoader[bossID]?.Invoke(darkWorldDifficulty));
                    break;
            }
        }

        private extern void orig_DoBossLevelSetup(int bossID, List<Pickups> treasure);

        private void FixedEmptyPortal() 
        {
            // allPortals isn't required, as it will not be empty anyway..
            FillIfNeeded(groundPortals, nameof(groundPortals));
            FillIfNeeded(nodePortals, nameof(nodePortals));
            FillIfNeeded(airPortals, nameof(airPortals));

            void FillIfNeeded(List<QuestSpawnPortal> portals, string portalName) 
            {
                if (portals.Count != 0)
                    return;
                
                var towerData = (Level.Session.MatchSettings.LevelSystem as DarkWorldLevelSystem).DarkWorldTowerData;
                var id = towerData.GetLevelID();
                portals.AddRange(allPortals);
            }
        }

        private void PlayBossMusic() 
        {
            foreach (var layer in Scene.Layers.Values) 
            {
                var boss = layer.GetFirst<patch_DarkWorldBoss>();
                if (boss is not null) 
                {
                    Music.Play(boss.BossMusic);
                    return;
                }
            }
        }
    }
}

namespace MonoMod 
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.PatchDarkWorldControlLevelSequence))]
    internal class PatchDarkWorldControlLevelSequence : Attribute {}

    internal static partial class MonoModRules 
    {
        public static void PatchDarkWorldControlLevelSequence(MethodDefinition method, CustomAttribute attrib) 
        {
            MethodDefinition complete = method.GetEnumeratorMoveNext();
            new ILContext(complete).Invoke(ctx => {
                var f__4this = ctx.Method.DeclaringType.FindField("<>4__this");
                var FixedEmptyPortal = f__4this.FieldType.Resolve().FindMethod("System.Void FixedEmptyPortal()");
                var PlayBossMusic = f__4this.FieldType.Resolve().FindMethod("System.Void PlayBossMusic()");
                var cursor = new ILCursor(ctx);
                ILLabel val = null;

                // It's BrFalse in Windows while BrTrue in Linux or MacOS
                Func<Instruction, bool> brFalseOrTrue;
                if (IsWindows)
                    brFalseOrTrue = instr => instr.MatchBrfalse(out val);
                else
                    brFalseOrTrue = instr => instr.MatchBrtrue(out val);

                cursor.GotoNext(brFalseOrTrue,
                    instr => instr.MatchLdstr("DarkBoss"));
                
                cursor.GotoNext();
                cursor.RemoveRange(2);

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Call, PlayBossMusic);

                cursor.MarkLabel(val);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f__4this);
                cursor.Emit(OpCodes.Call, FixedEmptyPortal);
            });
        }
    }
}