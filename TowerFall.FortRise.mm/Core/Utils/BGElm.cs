using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace FortRise;

public static class BGElm
{
    public static BGLayer Backdrop(string texture, Vector2 position = default, float opacity = 1)
    {
        return new()
        {
            Name = "Backdrop",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["opacity"] = opacity
            },
            SingleChildren = texture
        };
    }

    public static BGLayer LightningFlash(Color color)
    {
        return new()
        {
            Name = "LightningFlash",
            Data = new Dictionary<string, object>()
            {
                ["color"] = color.ColorToRGBHex()
            }
        };
    }

    public static BGLayer VortexLayer()
    {
        return new()
        {
            Name = "VortexLayer"
        };
    }

    public static BGLayer SacredGroundMoonLayer(Vector2 position = default)
    {
        return new()
        {
            Name = "SacredGroundMoonLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y
            }
        };
    }

    public static BGLayer FlightMoonLayer(Vector2 position = default)
    {
        return new()
        {
            Name = "FlightMoonLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y
            }
        };
    }

    public static BGLayer GhostShipLayer(Vector2 position = default, float scale = 1)
    {
        return new()
        {
            Name = "GhostShipLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["scale"] = scale
            }
        };
    }

    public static BGLayer OceanLayer(IBGSpriteContainerEntry waterSplash, int timeMin = 120, int timeAdd = 660, Vector2 position = default, float opacity = 1)
    {
        return new()
        {
            Name = "OceanLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["timeMin"] = timeMin,
                ["timeAdd"] = timeAdd,
                ["opacity"] = opacity
            },
            SingleChildren = waterSplash.Entry.ID
        };
    }

    public static BGLayer ScrollLayer(ISubtextureEntry bgTexture, Vector2 position = default, int speedX = 0, int speedY = 0, float opacity = 1, bool tileX = true, bool tileY = true)
    {
        return new()
        {
            Name = "ScrollLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["speedX"] = speedX,
                ["speedY"] = speedY,
                ["tileX"] = tileX,
                ["tileY"] = tileY,
                ["opacity"] = opacity
            },
            SingleChildren = bgTexture.ID
        };
    }

    public static BGLayer FadeLayer(IBGSpriteContainerEntry bgSprite, int xRange, int yRange, float fadeSpeed = 0.02f, Vector2 position = default, float opacity = 0.5f, float opacityRange = 0.25f)
    {
        return new()
        {
            Name = "FadeLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["xRange"] = xRange,
                ["yRange"] = yRange,
                ["fadeSpeed"] = fadeSpeed,
                ["opacityRange"] = opacityRange,
                ["opacity"] = opacity
            },
            SingleChildren = bgSprite.Entry.ID
        };
    }

    public static BGLayer FadeMoveLayer(IBGSpriteContainerEntry bgSprite, int xRange, int yRange, float time = 1f, float rateRange = 0.4f, Vector2 position = default, float maxAlpha = 1f)
    {
        return new()
        {
            Name = "FadeMoveLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["xRange"] = xRange,
                ["yRange"] = yRange,
                ["rateRange"] = rateRange,
                ["time"] = time,
                ["maxAlpha"] = maxAlpha 
            },
            SingleChildren = bgSprite.Entry.ID
        };
    }

    public static BGLayer WavyLayer(ISubtextureEntry bgTexture, Vector2 position = default, float opacity = 1, int waveFrames = 60, float amplitude = 2f, int sliceSize = 10, float sliceAdd = 0.125f, bool horizontal = false)
    {
        return new()
        {
            Name = "WavyLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["waveFrames"] = waveFrames,
                ["amplitude"] = amplitude,
                ["sliceSize"] = sliceSize,
                ["sliceAdd"] = sliceAdd,
                ["horizontal"] = horizontal,
                ["opacity"] = opacity
            },
            SingleChildren = bgTexture.ID
        };
    }

    public static BGLayer AnimatedLayer(IBGSpriteContainerEntry bgSprite, Vector2 position = default, float opacity = 1f)
    {
        return new()
        {
            Name = "AnimatedLayer",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["opacity"] = opacity
            },
            SingleChildren = bgSprite.Entry.ID
        };
    }

    public static BGLayer NodeLayer(IBGSpriteContainerEntry sprite, BGLayer[] nodeLayers, bool endWarp = true)
    {
        return new()
        {
            Name = "NodeLayer",
            Data = new Dictionary<string, object>()
            {
                ["sprite"] = sprite.Entry.ID,
                ["endWarp"] = endWarp
            },
            Childrens = nodeLayers
        };
    }

    public static BGLayer Node(int xRange, int yRange, int animID = 0, float speed = 1f, float speedRange = 0f, int delay = 0, int delayRange = 0, Vector2 position = default, float opacity = 1f)
    {
        return new()
        {
            Name = "Node",
            Data = new Dictionary<string, object>()
            {
                ["x"] = position.X,
                ["y"] = position.Y,
                ["xRange"] = xRange,
                ["yRange"] = yRange,
                ["animID"] = animID,
                ["speed"] = speed,
                ["speedRange"] = speedRange,
                ["delay"] = delay,
                ["delayRange"] = delayRange,
                ["opacity"] = 1
            }
        };
    }

    public static BGLayer Random(BGLayer[] childrens, float chance)
    {
        return new()
        {
            Name = "Random",
            Data = new Dictionary<string, object>()
            {
                ["chance"] = chance
            },
            Childrens = childrens
        };
    }

    public static BGLayer Overtime(BGLayer[] childrens)
    {
        return new()
        {
            Name = "Overtime",
            Childrens = childrens
        };
    }

    public static BGLayer Else(BGLayer[] childrens)
    {
        return new()
        {
            Name = "Else",
            Childrens = childrens
        };
    }
}