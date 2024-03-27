using System.Collections.Generic;
using System.Xml;
using MonoMod;

namespace TowerFall.Editor;

public class patch_ActorsLayer : ActorsLayer
{
    public patch_ActorsLayer(Level level) : base(level)
    {
    }

    [MonoModIgnore]
    [MonoModLinkTo("TowerFall.Editor.Layer", "System.Void .ctor(TowerFall.Editor.Level)")]
    public void base_ctor(Level level) 
    {
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Level level, XmlElement xml) 
    {
        base_ctor(level);
        this.Actors = new List<Actor>();
        foreach (var obj in patch_ActorData.DataLayers) 
        {
            ActorData.Data = obj;
            foreach (XmlElement xmlElement in xml) 
            {
                string text;
                if (xmlElement.Name == "TeamSpawnA" || xmlElement.Name == "TeamSpawnB")
                {
                    text = "TeamSpawn";
                }
                else
                {
                    text = xmlElement.Name;
                }

                if (obj.ContainsKey(text)) 
                {
                    var actor = new Actor(base.Level, xmlElement, text);
                    Actors.Add(actor);
                    TotalWeight += actor.Data.Weight;
                }
            }
        }
        ActorData.Data = patch_ActorData.DataLayers[0];
    }
}