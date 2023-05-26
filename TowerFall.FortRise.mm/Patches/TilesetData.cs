using System.IO;
using System.Xml;
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

    public static patch_TilesetData Create(XmlElement xml, string pathToImage) 
    {
        var tilesetData = new patch_TilesetData();
        using var fs = File.OpenRead(pathToImage);
        var texture2D = Texture2D.FromStream(Engine.Instance.GraphicsDevice, fs);
        tilesetData.Texture = new Subtexture(new Monocle.Texture(texture2D));
        tilesetData.AutotileData = new AutotileData(xml);
        return tilesetData;
    }
}