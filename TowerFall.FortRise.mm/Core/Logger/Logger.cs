using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Monocle;

namespace FortRise;

public static class Logger 
{
    public enum LogLevel { Debug, Warning, Error, Assert, Info }
    private static StringBuilder builder = new();
    private static IConsole consoleWindow;

    public static LogLevel Verbosity = LogLevel.Info;

    public static void AttachConsole(IConsole window) 
    {
        consoleWindow = window;
        consoleWindow.Attach();
    }

    public static void DetachConsole() 
    {
        consoleWindow?.Detach();
    }

    private static void LogInternal(LogLevel level, string message, int lineNumber) 
    {
        if (Verbosity < level)
            return;

        var logName = level switch 
        {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Assert => "[ASSERT]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Error => "[ERROR]",
            _ => "[INFO]"
        };
        var text = $"{logName} Ln: {lineNumber} {message}";

        builder.AppendLine(text);
        Engine.Instance?.Commands?.Log(text);
        if (consoleWindow != null)
            consoleWindow.StdOut.WriteLine(text);
        if (level == LogLevel.Error || level == LogLevel.Assert)
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
        var directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteToFile(fs);
    }

    public static void WriteToFile(Stream stream) 
    {
        var texts = builder.ToString();
        if (!string.IsNullOrEmpty(texts))
            return;
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(texts);
    }
}