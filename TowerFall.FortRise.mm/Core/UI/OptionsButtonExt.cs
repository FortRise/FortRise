using MonoMod.Utils;
using TowerFall;

namespace FortRise;

public static class OptionsButtonExt
{
    private const string Key = "fortrise_description";

    extension (OptionsButton button)
    {
        public string Description 
        {
            get => DynamicData.For(button).TryGet<string>(Key, out var description) ? description : null;
            set => DynamicData.For(button).Set(Key, value);
        }
    }
}
