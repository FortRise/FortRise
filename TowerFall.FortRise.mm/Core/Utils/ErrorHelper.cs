using System;
using System.Collections.Generic;
using Monocle;
using TowerFall;

namespace FortRise;

public static class ErrorHelper 
{
    public static Dictionary<string, Exception> StoredException = new();
    public static Exception LastException;
    public static string LastLessDetailedException;

    public static void ShowError(this Scene scene, string exceptionName) 
    {
        scene.Add(CreateLessDetailError(exceptionName, scene));
    }

    private static UIModal CreateLessDetailError(string exceptionName, Scene scene) 
    {
        return new UIModal(-1) { AutoClose = true }
            .HideTitle(true)
            .AddFiller("ERROR!")
            .AddFiller(exceptionName)
            .AddItem("Close", () => {
                Engine.Instance.Scene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            })
            .AddItem("More", () => {
                scene.Add(CreateFullDetailError(exceptionName, scene));
            });
    }

    private static UIModal CreateFullDetailError(string exceptionName, Scene scene) 
    {
        var uiModal = new UIModal(-1) { AutoClose = true }.HideTitle(true);
        var lines = ErrorHelper.GetExceptionStringLine(exceptionName);
        for (int i = 0; i < lines.Length; i++) 
        {
            var line = lines[i];
            uiModal.AddFiller(line);
        }
        uiModal.AddItem("Close", () => {
                Engine.Instance.Scene = new MapScene(MainMenu.RollcallModes.DarkWorld);
            })
            .AddItem("Less", () => {
                scene.Add(CreateLessDetailError(exceptionName, scene));
            });
        return uiModal;
    }

    public static void StoreException(string exceptionName, Exception exception) 
    {
        LastException = exception;
        LastLessDetailedException = exceptionName;
        StoredException[exceptionName] = exception;
    }

    public static void StoreException(Exception exception) 
    {
        LastException = exception;
    }

    public static Exception GetLastException() 
    {
        return LastException;
    }

    public static string[] GetExceptionStringLine(string exceptionName) 
    {
        var str = GetExceptionString(exceptionName);
        return str.Split('\n');
    }

    public static string GetLastExceptionString() 
    {
        if (LastException == null)
            return null;
        return LastException.ToString();
    }

    public static string GetLastLessDetailedException() 
    {
        return LastLessDetailedException;
    }

    public static string GetExceptionString(string exceptionName) 
    {
        if (StoredException.TryGetValue(exceptionName, out Exception val)) 
        {
            return val.ToString();
        }
        return null;
    }

    public static Exception GetException(string exceptionName) 
    {
        if (StoredException.TryGetValue(exceptionName, out Exception val)) 
        {
            return val;
        }
        return null;
    }
}