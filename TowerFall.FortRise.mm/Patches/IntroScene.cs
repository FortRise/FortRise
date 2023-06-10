using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_IntroScene : IntroScene 
{
    [MonoModReplace]
    private Subtexture GetLetterSub(int letterIndex) 
    {
        if (patch_TFGame.FortRiseMenuAtlas.Contains("mmg/" + letterIndex))
            return patch_TFGame.FortRiseMenuAtlas["mmg/" + letterIndex];
        return TFGame.MenuAtlas["mmg/" + letterIndex];
    }
}