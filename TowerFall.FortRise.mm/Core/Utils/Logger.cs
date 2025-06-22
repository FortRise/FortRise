using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Monocle;

namespace FortRise;

internal static class Logger
{
    public enum LogLevel { Info, Warning, Error, Debug, Verbose, Assert}
    private static StringBuilder builder = new();

    public static LogLevel Verbosity = LogLevel.Info;


    private static void LogInternal(LogLevel level, string message, int lineNumber)
    {
        if (Verbosity < level)
            return;

        var text = message;

        builder.AppendLine(text);

        switch (level)
        {
            case LogLevel.Info:
                RiseCore.logger.LogInformation(text);
                break;
            case LogLevel.Warning:
                RiseCore.logger.LogWarning(text);
                break;
            case LogLevel.Assert:
                RiseCore.logger.LogCritical(text);
                break;
            case LogLevel.Error:
                RiseCore.logger.LogError(text);
                break;
            case LogLevel.Debug:
            case LogLevel.Verbose:
                RiseCore.logger.LogDebug(text);
                break;
        }

        try
        {
            Engine.Instance?.Commands?.Log(text);
        }
        catch (ArgumentOutOfRangeException)
        {
        }
        catch (ArgumentException)
        {
        }

        if (level == LogLevel.Assert)
            Debugger.Break();
    }

    public static void Log(
        string log,
        LogLevel logLevel = LogLevel.Debug,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        LogInternal(logLevel, log, callerLineNumber);
    }

    public static void Log(
        object log,
        LogLevel logLevel = LogLevel.Debug,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        string message = log switch
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };
        LogInternal(logLevel, message, callerLineNumber);
    }

    [Conditional("DEBUG")]
    public static void Assert(
        bool condition,
        string message,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        if (!condition)
            LogInternal(LogLevel.Assert, message, callerLineNumber);
    }

    public static void Info(
        string log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        LogInternal(LogLevel.Info, log, callerLineNumber);
    }


    public static void Info(
        object log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        string message = log switch
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };

        LogInternal(LogLevel.Info, message, callerLineNumber);
    }

    public static void Warning(
        string log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        LogInternal(LogLevel.Warning, log, callerLineNumber);
    }


    public static void Warning(
        object log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        string message = log switch
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };

        LogInternal(LogLevel.Warning, message, callerLineNumber);
    }

    public static void Verbose(
        string log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        LogInternal(LogLevel.Verbose, log, callerLineNumber);
    }


    public static void Verbose(
        object log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        string message = log switch
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };

        LogInternal(LogLevel.Verbose, message, callerLineNumber);
    }

    public static void Error(
        string log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        LogInternal(LogLevel.Error, log, callerLineNumber);
    }


    public static void Error(
        object log,
        [CallerLineNumber] int callerLineNumber = 0
    )
    {
        string message = log switch
        {
            null => "null",
            _ => log.ToString() ?? "null"
        };

        LogInternal(LogLevel.Error, message, callerLineNumber);
    }

#if !ANDROID && !Blazor
    public static void OpenLog(string path)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            }
        };
        if (File.Exists(path))
            process.Start();
    }
#endif

    public static void WriteToFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteToFile(fs);
    }

    public static void WriteToFile(Stream stream)
    {
        var texts = builder.ToString();
        if (string.IsNullOrEmpty(texts))
            return;
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(texts);
    }
}
