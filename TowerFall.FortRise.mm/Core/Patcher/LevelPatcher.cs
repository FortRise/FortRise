using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TowerFall;
using Monocle;

// TODO Implement? or not?

namespace FortRise;

public static class TowerPatcher 
{
    public static VersusPatchCategory Versus;
    public static QuestPatchCategory Quest;
    public static DarkWorldPatchCategory DarkWorld;
}

public abstract class TowerPatchCategory<T>
{
    internal Dictionary<string, Action<OpenedTower<T>>> Modifications;
    public abstract string Path { get; }
    public virtual string DLCPath { get; }
    public abstract void Patch(string level, Action<OpenedTower<T>> modification);
    public abstract void LoadTower(string level, XmlElement xml);

    protected void AddModification(string level, Action<OpenedTower<T>> modification) 
    {
        Modifications ??= new();
        Modifications.Add(level, modification);
    }
}

public abstract class OpenedTower<T>
{
    public T Data;

    public OpenedTower(XmlElement xmlToModify) 
    {
    }
}

public class QuestOpenedTower : OpenedTower<XmlElement>
{
    public QuestOpenedTower(XmlElement xmlToModify) : base(xmlToModify)
    {
        TowerPatcher.Quest.Patch(QuestPatchCategory.SACRED_GROUND, (OpenedTower<XmlElement> x) => {
            var quest = x as QuestOpenedTower;

            // Wave 6
            var wave6 = quest.AddWave();
            var group1w6 = wave6.AddGroup();
                group1w6.AddSpawn("TopLeft");
                group1w6.AddSpawn("BotRight");
                group1w6.AddEnemy("[2]Skeleton");
            var group2w6 = wave6.AddGroup();
                group2w6.AddSpawn("BotLeft, BotRight");
                group2w6.AddSpawn("BotLeft");
                group2w6.AddEnemy("[2]Slime");
                group2w6.AddEnemy("[2]ScytheCultist");
            var group3w6 = wave6.AddGroup();
                group3w6.AddSpawn("TopRight, TopLeft");
                group3w6.AddEnemy("[2]ScytheCultist");
            
            // Wave 7
            var wave7 = quest.AddWave();
            var group1w7 = wave7.AddGroup();
                group1w7.AddEnemy("[2]Slime");
                group1w7.AddSpawn("TopLeft");
                group1w7.AddSpawn("TopRight");
        });
    }


    public Wave AddWave() 
    {
        return default;
    }

    public Wave GetWave(int waveNumber, QuestTowerDifficulty difficulty) 
    {
        Wave wave = new Wave();
        switch (difficulty) 
        {
        case QuestTowerDifficulty.Normal:
            wave.Xml = Data["normal"].GetElementsByTagName("wave")[waveNumber] as XmlElement;
            break;
        default:
            wave.Xml = Data["hardcore"].GetElementsByTagName("wave")[waveNumber] as XmlElement;
            break;
        }
        return wave;
    }

    public struct Wave 
    {
        public XmlElement Xml;

        public Group AddGroup() 
        {
            var groupXml = Xml.CreateChild("group");
            var group = new Group();
            group.Xml = groupXml;
            return group;;
        }

        public void SetDark(bool dark) 
        {
            Xml.SetAttribute("dark", dark.ToString());
        }

        public void SetSlow(bool slow) 
        {
            Xml.SetAttribute("slow", slow.ToString());
        }

        public void SetFloor(int x) 
        {
            if (Xml.HasChild("floors")) 
            {
                Xml["floors"].InnerText = x.ToString();
                return;
            }
            var child = Xml.CreateChild("floors");
            child.InnerText = x.ToString();
        }

        public Group GetGroup(int groupId) 
        {
            var group = new Group();
            group.Xml = Xml.GetElementsByTagName("group")[groupId] as XmlElement;
            return group;
        }
    }

    public struct Group 
    {
        public XmlElement Xml;

        public void SetDelay(int delay) 
        {
            Xml.SetAttribute("delay", delay.ToString());
        }

        public void IsCoop(bool isCoop) 
        {
            Xml.SetAttribute("coop", isCoop.ToString());
        }

        public void IsSolo(bool isSolo) 
        {
            Xml.SetAttribute("solo", isSolo.ToString());
        }

        public void AddReaper(int reaperDifficulty) 
        {
            var reaper = Xml.CreateChild("reaper");
            reaper.InnerText = reaperDifficulty.ToString();
        }

        public void AddSpawn(string portalSpawn) 
        {

        }

        public void AddTreasure(string treasure) 
        {

        }

        public void AddEnemy(string enemy) 
        {

        }
    }
}

public enum QuestTowerDifficulty 
{
    Normal, Hardcore
}

public class DarkWorldOpenedTower : OpenedTower<DarkWorldTowerData>
{
    public DarkWorldOpenedTower(XmlElement xmlToModify) : base(xmlToModify)
    {
    }
}

public class VersusOpenedTower : OpenedTower<VersusTowerData>
{
    public VersusOpenedTower(XmlElement xmlToModify) : base(xmlToModify)
    {
    }

    public void AddTreasure(string treasure) 
    {
    }
}

public sealed class QuestPatchCategory : TowerPatchCategory<XmlElement>
{
    public const string SACRED_GROUND = "00";
    public const string TWILIGHT_SPIRE = "01";
    public const string BACKFIRE = "02";
    public const string FLIGHT = "03";
    public const string MIRAGE = "04";
    public const string THORNWOOD = "05";
    public const string FROSTFANG_KEEP = "06";
    public const string KINGS_COURT = "07";
    public const string SUNKEN_CITY = "08";
    public const string MOONSTONE = "09";
    public const string TOWERFORGE = "10";
    public const string ASCENSION = "11";
    public const string GAUNTLET = "12";
    public const string GAUNTLET_II = "13";

    public override string Path => "Content/Levels/Quest"; 

    public override void LoadTower(string level, XmlElement xml)
    {
    }

    public override void Patch(string level, Action<OpenedTower<XmlElement>> modification)
    {
        var towerPath = System.IO.Path.Combine(Path, level + "data.xml");
        if (!File.Exists(towerPath)) 
        {
            Logger.Error($"[LevelPatch] File {towerPath} does not exists!");
            return;
        }
        AddModification(level, modification);
    }
}

public sealed class VersusPatchCategory : TowerPatchCategory<VersusTowerData>
{
    public const string SACRED_GROUND = "00 - Sacred Ground";
    public const string TWILIGHT_SPIRE = "01 - Twilight Spire";
    public const string BACKFIRE = "02 - Backfire";
    public const string FLIGHT = "03 - Flight";
    public const string MIRAGE = "04 - Mirage";
    public const string THORNWOOD = "05 - Thornwood";
    public const string FROSTFANG_KEEP = "06 - Frostfang Keep";
    public const string KINGS_COURT = "07 - Kings Court";
    public const string SUNKEN_CITY = "08 - Sunken City";
    public const string MOONSTONE = "09 - Moonstone";
    public const string TOWERFORGE = "10 - TowerForge";
    public const string ASCENSION = "11 - Ascension";
    public const string THE_AMARANTH = "12 - The Amaranth";
    public const string DREADWOOD = "13 - Dreadwood";
    public const string DARKFANG = "14 - Darkfang";
    public const string CATACLYSM = "15 - Cataclysm";

    public override string Path => "Content/Levels/Versus";
    public override string DLCPath => "DarkWorldContent/Levels/Versus";

    public override void LoadTower(string level, XmlElement xml)
    {
        var data = Modifications[level];
        var path = System.IO.Path.Combine(DLCPath, level);
        if (!File.Exists(path))
            return;
        var files = Directory.GetFiles(path);
        var openedTower = new VersusOpenedTower(xml);
        data?.Invoke(openedTower);
    }

    public override void Patch(string level, Action<OpenedTower<VersusTowerData>> modification)
    {
        var towerPath = System.IO.Path.Combine(Path, level);
        if (!Directory.Exists(towerPath)) 
        {
            Logger.Error($"[LevelPatch] Folder {towerPath} does not exists!");
            return;
        }
        AddModification(level, modification);
    }
}

public sealed class DarkWorldPatchCategory : TowerPatchCategory<DarkWorldTowerData>
{
    public const string THE_AMARANTH = "0 - The Amaranth";
    public const string DREADWOOD = "1 - Dreadwood";
    public const string DARKFANG = "2 - Darkfang";
    public const string CATACLYSM = "3 - Cataclysm";
    public const string DARK_GAUNTLET = "4 - DarkGauntlet";

    public override string Path => "Content/Levels/DarkWorld";
    public override string DLCPath => "DarkWorldContent/Levels/DarkWorld";

    public override void LoadTower(string level, XmlElement xml)
    {
        var data = Modifications[level];
        var path = System.IO.Path.Combine(DLCPath, level);
        if (!File.Exists(path))
            return;
        var files = Directory.GetFiles(path);
        var openedTower = new DarkWorldOpenedTower(xml);
        data?.Invoke(openedTower);
    }

    public override void Patch(string level, Action<OpenedTower<DarkWorldTowerData>> modification)
    {
        var towerPath = System.IO.Path.Combine(Path, level);
        if (!Directory.Exists(towerPath)) 
        {
            Logger.Error($"[LevelPatch] Folder {towerPath} does not exists!");
            return;
        }
        AddModification(towerPath, modification);
    }
}