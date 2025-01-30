using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_DarkWorldTowerData : DarkWorldTowerData 
{
    public struct Variant 
    {
        // Could prevent from marking it as field?
        public Dictionary<string, bool> CustomVariants { get; set; }
    }

    [MonoModIgnore]
    [MonoModPublic]
    private extern List<DarkWorldTowerData.LevelData> LoadLevelSet(XmlElement xml);

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.DarkWorldTowerData", "LoadLevelSet")]
    public List<DarkWorldTowerData.LevelData> LoadLevelSet_Public(XmlElement xml) 
    {
        return LoadLevelSet(xml);
    }

    public class patch_LevelData : LevelData 
    {
        public static string[] OriginalVariantNames;
        public Variant ActiveVariant;
        public string CustomBossName = string.Empty;

        public patch_LevelData(XmlElement xml, Dictionary<string, List<EnemyData>> enemySets) : base(xml, enemySets)
        {
        }


        public extern void orig_ctor(XmlElement xml, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets);

        [MonoModConstructor]
        public void ctor(XmlElement xml, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets) 
        {
            if (xml.HasChild("variants")) 
            {
                XmlToVariant(xml["variants"]);
            }
            orig_ctor(xml, enemySets);
            if (xml.HasChild("customboss")) 
            {
                CustomBossName = xml.ChildText("customboss");
                LevelMode = DarkWorldTowerData.LevelData.BossModes.Boss;
            }
        }

        private void AddTreasure(string treasure) 
        {
            var treasureSpan = treasure.AsSpan();
            int inc = 0;
            int dec = 0;
            while (treasureSpan[0] == '+') 
            {
                treasureSpan = treasureSpan.Slice(1);
                inc++;
            }
            while (treasureSpan[0] == '-') 
            {
                treasureSpan = treasureSpan.Slice(1);
                dec++;
            }
            
            if (dec == 0) 
            {
                dec = 4;
            }
            Pickups pickups = Calc.StringToEnum<Pickups>(treasureSpan.ToString());
            for (int k = 0; k < 4; k++)
            {
                if (k >= inc - 1 && k <= dec - 1)
                {
                    this.TreasureData[k].Add(pickups);
                }
            }
        }

        [PostPatchXmlToVariant]
        public void XmlToVariant(XmlElement xml) 
        {
            ActiveVariant.CustomVariants ??= new Dictionary<string, bool>();
            if (OriginalVariantNames == null) 
            {
                var fields = typeof(Variant).GetFields(BindingFlags.Public | BindingFlags.Instance);
                OriginalVariantNames = new string[fields.Length];
                for (int i = 0; i < OriginalVariantNames.Length; i++) 
                {
                    var field = fields[i];
                    OriginalVariantNames[i] = field.Name;
                }
            }
            for (int i = 0; i < xml.ChildNodes.Count; i++) 
            {
                var child = xml.ChildNodes[i];
                if (OriginalContains(child.Name))
                    continue;
                ActiveVariant.CustomVariants.Add(child.Name, bool.Parse(child.InnerText));
            }
        }

        public static bool OriginalContains(string name) 
        {
            for (int i = 0; i < OriginalVariantNames.Length; i++) 
            {
                if (OriginalVariantNames[i] == name)
                    return true;
            }
            return false;
        }
    }

    public class patch_EnemyData : EnemyData
    {
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
        }
    }
}
