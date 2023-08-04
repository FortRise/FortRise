using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod;
using NLua;

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

        public patch_LevelData(LuaTable table, Dictionary<string, List<EnemyData>> enemySets) : base(null, enemySets)
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

        [MonoModConstructor]
        public void ctor(LuaTable table, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets) 
        {
            LuaCtor(table, enemySets);
            if (table.TryGet("customboss", out var customBoss)) 
            {
                CustomBossName = customBoss;
                LevelMode = DarkWorldTowerData.LevelData.BossModes.Boss;
            }
        }

        public void LuaCtor(LuaTable table, Dictionary<string, List<DarkWorldTowerData.EnemyData>> enemySets) 
        {
            this.File = table.GetInt("file");
            this.Difficulty = table.GetInt("difficulty");
            this.Waves = table.GetInt("waves", 3);
            enemySets.TryGetValue(table.Get("enemySet", ""), out this.EnemySet);
            this.DelayMultiplier = table.GetFloat("delayMultiplier", 1f);
            this.TreasureData = new List<Pickups>[4];
            for (int i = 0; i < 4; i++)
            {
                this.TreasureData[i] = new List<Pickups>();
            }
            if (table.TryGetTable("treasure", out var csv))
            {
                foreach (KeyValuePair<object, object> obj in csv) 
                {
                    var str = (string)obj.Value;
                    int increment = 0;
                    int decrement = 0;
                    while (str[0] == '+') 
                    {
                        str = str.Substring(1);
                        increment++;
                    }

                    while (str[0] == '-') 
                    {
                        str = str.Substring(1);
                        decrement++;
                    }
                    if (decrement == 0)
                    {
                        decrement = 4;
                    }
                    Pickups pickups = Calc.StringToEnum<Pickups>(str);
                    for (int k = 0; k < 4; k++)
                    {
                        if (k >= increment - 1 && k <= decrement - 1)
                        {
                            this.TreasureData[k].Add(pickups);
                        }
                    }
                }
            }
            if (table.TryGetTable("boss", out var boss))
            {
                this.LevelMode = DarkWorldTowerData.LevelData.BossModes.Boss;
                this.BossID = boss.GetInt("boss", 0);
                return;
            }
            this.LevelMode = DarkWorldTowerData.LevelData.BossModes.Normal;
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

        public patch_EnemyData(LuaTable table) 
            // This is basically useless
            : base(new patch_EnemyData(new XmlDocument().GetElementById("null")))
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
        public void ctor(LuaTable data) 
        {
            Enemy = data.Get("enemy");
            Type = data.GetEnum<PortalTypes>("type");
            Delay = data.GetInt("delay");
            Difficulty = data.GetInt("difficulty");
            Weight = data.GetFloat("weight");
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