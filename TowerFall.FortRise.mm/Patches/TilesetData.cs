using System.IO;
using System.Xml;
using FortRise;
using FortRise.Adventure;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_TilesetData : TilesetData
{
    public AutotileData AutotileData 
    {  
        [MonoModIgnore]
        get => null; 
        [MonoModIgnore]
        private set => throw new System.Exception(value.ToString()); 
    }

    public Subtexture Texture
    {  
        [MonoModIgnore]
        get => null; 
        [MonoModIgnore]
        private set => throw new System.Exception(value.ToString()); 
    }
    [MonoModConstructor]
    internal void ctor() {}

    internal patch_TilesetData() : base(null) {}

    public patch_TilesetData(XmlElement xml, ThemeResource resource) : base(null) {}

    [MonoModConstructor]
    public void ctor(XmlElement xml, ThemeResource resource) 
    {
        var image = xml.Attr("image");
        if (resource.Atlas.Contains(image))
        {
            Texture = resource.Atlas[image];
        }
        AutotileData = new AutotileData(xml);
    }

    public static patch_TilesetData Create(XmlElement xml, string pathToImage) 
    {
        var tilesetData = new patch_TilesetData();
        using var fs = File.OpenRead(pathToImage);
        var texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
        tilesetData.Texture = new Subtexture(new Monocle.Texture(texture2D));
        tilesetData.AutotileData = new AutotileData(xml);
        return tilesetData;
    }

    public static patch_TilesetData Create(XmlElement xml, Stream fs) 
    {
        var tilesetData = new patch_TilesetData();
        var texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
        tilesetData.Texture = new Subtexture(new Monocle.Texture(texture2D));
        tilesetData.AutotileData = new AutotileData(xml);
        return tilesetData;
    }
}