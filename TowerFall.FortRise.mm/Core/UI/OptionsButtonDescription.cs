using MonoMod.Utils;
using TowerFall;

namespace FortRise;

public static class OptionsButtonDescription
{
    private const string Key = "fortrise_description";

    public static void SetDescription(this OptionsButton button, string description)
    {
        DynamicData.For(button).Set(Key, description);
    }

    public static string GetDescription(this OptionsButton button)
    {
        return DynamicData.For(button).TryGet<string>(Key, out var description) ? description : null;
    }
}
