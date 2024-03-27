using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FortRise;

public static class TaskHelper 
{
    private static ConcurrentDictionary<string, object> taskMap = new();
    public static ICollection<string> Tasks => taskMap.Keys;

    public static void Erase(string id) 
    {
        if (!taskMap.TryRemove(id, out _))
            throw new Exception("Task cancellation failed!");
    }

    public static Task Run(string id, Action func) 
    {
        var task = taskMap.GetOrAdd(id, id => {
            return Task.Run(() => {
                func?.Invoke();
                Erase(id);
            });
        });
        return (Task)task;
    }

    public static Task RunAsync(string id, Func<Task> func) 
    {
        var task = taskMap.GetOrAdd(id, id => {
            return Task.Run(async () => {
                await func?.Invoke();
                Erase(id);
            });
        });
        return (Task)task;
    }

    public static bool Wait(string id) 
    {
        return !taskMap.ContainsKey(id);
    }

    public static bool WaitForAll() 
    {
        return taskMap.Count != 0;
    }
}