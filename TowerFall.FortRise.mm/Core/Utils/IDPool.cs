using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FortRise;

public static class IDPool
{
    public static Dictionary<string, int> PooledID = new Dictionary<string, int>();
    public static Dictionary<string, HashSet<int>> Freed = new Dictionary<string, HashSet<int>>();

    public static int Obtain(string id)
    {
        ref var freedPool = ref CollectionsMarshal.GetValueRefOrAddDefault(Freed, id, out bool freedExists);
        if (freedExists && freedPool.Count != 0)
        {
            var first = freedPool.First();
            freedPool.Remove(first);
            return first;
        }

        ref var pool = ref CollectionsMarshal.GetValueRefOrAddDefault(PooledID, id, out bool exists);
        if (!exists)
        {
            pool = 0;
            Freed[id] = new HashSet<int>();
            return pool;
        }

        pool += 1;
        return pool;
    }

    public static void Free(string id, int toFree)
    {
        ref var freedPool = ref CollectionsMarshal.GetValueRefOrNullRef(Freed, id);
        if (Unsafe.IsNullRef(ref freedPool))
        {
            Logger.Error($"[IDPool] The id: {id} does not exists for {toFree}.");
            return;
        }

        freedPool.Add(toFree);
    }
}