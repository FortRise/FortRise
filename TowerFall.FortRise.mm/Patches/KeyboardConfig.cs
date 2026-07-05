using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;
using MonoMod;

namespace TowerFall.Patching;

[MonoModPatch("TowerFall.KeyboardConfig")]
public class KeyboardConfig
{
    [JsonInclude]
    public Keys[] Left;

    [JsonInclude]
    public Keys[] Right;

    [JsonInclude]
    public Keys[] Up;

    [JsonInclude]
    public Keys[] Down;

    [JsonInclude]
    public Keys[] Jump;

    [JsonInclude]
    public Keys[] Shoot;

    [JsonInclude]
    public Keys[] AltShoot;

    [JsonInclude]
    public Keys[] Dodge;

    [JsonInclude]
    public Keys[] Arrows;

    [JsonInclude]
    public Keys[] MenuAlt;

    [JsonInclude]
    public Keys[] Start;
}