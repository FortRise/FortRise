using System;
using System.Collections.Generic;

namespace FortRise;

public static class ExtendedGameData 
{
    public static IReadOnlyDictionary<string, MapRendererData> MapRenderers => InternalMapRenderers;

    internal static Dictionary<string, MapRendererData> InternalMapRenderers = new();
}
