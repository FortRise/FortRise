using System.Xml;

namespace TowerFall;

public class patch_Level : Level
{
    public patch_Level(Session session, XmlElement xml) : base(session, xml)
    {
    }

    public extern void orig_LoadEntity(XmlElement e);

    public void LoadEntity(XmlElement e) 
    {
        var name = e.Name;
        if (FortRise.RiseCore.LevelEntityLoader.TryGetValue(name, out var val)) 
        {
            Add(val(e));
            return;
        }
        orig_LoadEntity(e);
    }
}