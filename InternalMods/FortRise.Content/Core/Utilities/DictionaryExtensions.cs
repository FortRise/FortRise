using System.Collections.Generic;

namespace FortRise.Content;

internal static class DictionaryExtensions 
{
    public static V? GetOrNull<K, V>(this IReadOnlyDictionary<K, V> dict, K key)
        where K : notnull
    {
        dict.TryGetValue(key, out var val);
        return val;
    }
}
