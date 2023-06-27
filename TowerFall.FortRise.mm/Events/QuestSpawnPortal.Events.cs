using FortRise;
using Microsoft.Xna.Framework;
using TowerFall;

namespace FortRise;

public static partial class RiseCore
{
    public static partial class Events 
    {
        public delegate bool GetArrowTypesHandler(string name, out ArrowTypes types);
        public static event GetArrowTypesHandler OnGetSkeletonArrowTypes;

        public delegate bool QuestSpawnPortal_FinishSpawnHandler(string entityName, Vector2 position, Facing facing, Level level);

        public static event QuestSpawnPortal_FinishSpawnHandler OnQuestSpawnPortal_FinishSpawn;

        internal static void InvokeQuestSpawnPortal_FinishSpawn(string name, Vector2 position, Facing facing, Level level) 
        {
            if (RiseCore.EnemyLoader.TryGetValue(name, out EnemyLoader loader)) 
            {
                level.Add(loader?.Invoke(position + new Vector2(0f, 2f), facing));
                return;
            }
            if (name.Contains("Skeleton") || name.Contains("Jester")) 
            {
                ArrowTypes arrows = ArrowTypes.Normal;
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
                
                arrows = GetSkeletonArrowTypes(name);
                level.Add(new Skeleton(position + new Vector2(0f, 2f), facing, arrows, hasShields, hasWings, canMimic, jester, boss));
                return;
            }
            var invoked = OnQuestSpawnPortal_FinishSpawn?.Invoke(name, position, facing, level);
            if (invoked == null || !invoked.Value)
            {
                Logger.Error($"Entity name: {name} failed to spawn as it does not exists!");
                Sounds.ui_levelLock.Play(160f);
                level.ScreenShake(8);
            }

            ArrowTypes GetSkeletonArrowTypes(string name) 
            {
                if (OnGetSkeletonArrowTypes != null && OnGetSkeletonArrowTypes(name, out ArrowTypes types)) 
                    return types;
                
                if (name.Contains("Bomb"))
                    return ArrowTypes.Bomb;
                if (name.Contains("SuperBomb"))
                    return ArrowTypes.SuperBomb;
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
                return ArrowTypes.Normal;
            }
        }
    }
}