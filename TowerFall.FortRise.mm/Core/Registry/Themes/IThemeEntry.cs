#nullable enable
using System.Xml;
using Microsoft.Xna.Framework;

namespace FortRise;

public interface IThemeEntry
{
    public string Name { get; init; }
    public ThemeConfiguration Configuration { get; init; }
}
