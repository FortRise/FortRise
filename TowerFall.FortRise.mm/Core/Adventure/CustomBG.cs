using Monocle;

namespace FortRise.Adventure;

public class CustomBGStorage 
{
    public readonly Atlas Atlas;
    public readonly SpriteData SpriteData;


    public CustomBGStorage(Atlas atlas, SpriteData spriteData) 
    {
        Atlas = atlas;
        SpriteData = spriteData;
    }
}