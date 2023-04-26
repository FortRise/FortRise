#pragma warning disable CS0626
#pragma warning disable CS0108
using System.Collections.Generic;
using System.Xml;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldTowerData : DarkWorldTowerData 
{
    public class patch_LevelData : LevelData 
    {
        public bool Dark;

        public patch_LevelData(XmlElement xml, Dictionary<string, List<EnemyData>> enemySets) : base(xml, enemySets)
        {
        }

        public extern void orig_ctor(XmlElement xml, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets);

        [MonoModConstructor]
        public void ctor(XmlElement xml, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets) 
        {
            if (xml.HasChild("dark")) 
            {
                Dark = xml.ChildBool("dark");
            }
            orig_ctor(xml, enemySets);
        }
    }

    public class patch_EnemyData : EnemyData
    {
        public ArrowTypes Arrow;

        public patch_EnemyData(XmlElement xml) : base(xml)
        {
        }

        [MonoModConstructor]
        [MonoModReplace]
        public void ctor(XmlElement xml) 
        {
            Enemy = xml.ChildText("enemy");
            Type = xml.ChildEnum<PortalTypes>("type");
            Delay = xml.ChildInt("delay");
            Difficulty = xml.ChildInt("difficulty");
            Weight = xml.ChildFloat("weight");
            if (xml.HasChild("arrow"))
                Arrow = xml.ChildEnum<ArrowTypes>("arrow");
            else 
                Arrow = ArrowTypes.Normal; 
        }

        [MonoModConstructor]
        [MonoModReplace]
        public void ctor(patch_EnemyData data) 
        {
            Enemy = data.Enemy;
            Type = data.Type;
            Delay = data.Delay;
            Difficulty = data.Difficulty;
            Weight = data.Weight;
            Arrow = data.Arrow;
        }
    }
}
