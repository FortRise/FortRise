using System.Xml;
using MonoMod;

namespace TowerFall;

public class patch_QuestLevelData : QuestLevelData
{
    public string Path;
    public string DataPath;
    public patch_QuestLevelData(int id, XmlElement xml) :base(0, null)
    {
    }

    public patch_QuestLevelData() :base(0, null)
    {
    }

    [MonoModConstructor]
    public void ctor() {}
}