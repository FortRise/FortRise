#nullable enable
using Monocle;
using TowerFall;

namespace FortRise;

public interface IModThemes
{
    IThemeEntry RegisterTheme(string id, ThemeConfiguration configuration);
    IThemeEntry? GetTheme(string id);
}

internal sealed class ModThemes : IModThemes
{
    private readonly RegistryQueue<IThemeEntry> registryQueue;
    private readonly ModuleMetadata metadata;

    internal ModThemes(ModuleMetadata metadata, ModuleManager manager)
    {
        this.metadata = metadata;
        registryQueue = manager.CreateQueue<IThemeEntry>(Invoke);
    }

    public IThemeEntry RegisterTheme(string id, ThemeConfiguration configuration)
    {
        string name = $"{metadata.Name}/{id}";

        IThemeEntry theme = new ThemeEntry(name, configuration);
        registryQueue.AddOrInvoke(theme);
        TowerThemeRegistry.AddTheme(theme);
        return theme;
    }

    public IThemeEntry? GetTheme(string id) => TowerThemeRegistry.GetTheme(id);
    

    private void Invoke(IThemeEntry entry)
    {
        var towerTheme = new patch_TowerTheme();
        towerTheme.Name = entry.Configuration.Name.ToUpperInvariant();
        towerTheme.ID = entry.Name;

        towerTheme.Icon = entry.Configuration.Icon?.Subtexture ?? TFGame.MenuAtlas["sacredGround"];
        towerTheme.TowerType = entry.Configuration.TowerType;
        towerTheme.MapPosition = entry.Configuration.MapPosition;
        towerTheme.Music = entry.Configuration.Music ?? "SacredGround";
        towerTheme.DarknessColor = entry.Configuration.DarknessColor.Invert();
        towerTheme.DarknessOpacity = entry.Configuration.DarknessOpacity;
        towerTheme.Wind = entry.Configuration.Wind;
        towerTheme.Lanterns = entry.Configuration.Lanterns;
        towerTheme.World = entry.Configuration.World;
        towerTheme.Raining = entry.Configuration.Raining;
        towerTheme.BackgroundID = entry.Configuration.BackgroundID ?? "SacredGround";

        if (GameData.BGs.ContainsKey(towerTheme.BackgroundID))
        {
            towerTheme.BackgroundData = GameData.BGs[towerTheme.BackgroundID]["Background"];
            towerTheme.ForegroundData = GameData.BGs[towerTheme.BackgroundID]["Foreground"];
        }

        towerTheme.DrillParticleColor = entry.Configuration.DrillParticleColor;
        towerTheme.Cold = entry.Configuration.Cold;
        towerTheme.CrackedBlockColor = entry.Configuration.CrackedBlockColor;
        towerTheme.Tileset = entry.Configuration.Tileset ?? "SacredGround";
        towerTheme.BGTileset = entry.Configuration.BGTileset ?? "SacredGroundBG";
        towerTheme.Cataclysm = entry.Configuration.Cataclysm;

        if (entry.Configuration.InvisibleOpacities == null)
        {
            towerTheme.InvisibleOpacities = [
                0.2f,
                0.2f,
                0.2f,
                0.2f,
                0.2f,
                0.2f,
                0.2f,
                0.2f,
                0.2f
            ];
        }
        else
        {
            towerTheme.InvisibleOpacities = entry.Configuration.InvisibleOpacities;

            for (int i = 0; i < towerTheme.InvisibleOpacities.Length; i++)
            {
                towerTheme.InvisibleOpacities[i] = 0.2f + towerTheme.InvisibleOpacities[i] * 0.1f;
            }
        }

        GameData.Themes[entry.Name] = towerTheme;
    }
}