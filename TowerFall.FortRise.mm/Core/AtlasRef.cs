using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise;

public class AtlasRef : patch_Atlas 
{
    private patch_Atlas atlasPtr;
    private string prefix;

    public Dictionary<string, Subtexture> Subtextures => atlasPtr.SubTextures;

    internal AtlasRef(patch_Atlas atlas, string prefix) 
    {
        atlasPtr = atlas;
        this.prefix = prefix;
    } 

    public Subtexture this[string name] 
    {
        get => atlasPtr[prefix + "/" + name];
    }

    public Subtexture this[string name, Rectangle subRect] 
    {
        get => atlasPtr[prefix + "/" + name, subRect];
    }
}