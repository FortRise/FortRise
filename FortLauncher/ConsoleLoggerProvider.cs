using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FortLauncher;

internal sealed class ConsoleLoggerProvider : ILoggerProvider
{
    private readonly TextWriter writer;
    private readonly LogLevel logLevel;
    public ConsoleLoggerProvider(LogLevel logLevel, TextWriter writer)
    {
        this.logLevel = logLevel;
        this.writer = writer;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new Logger(this, logLevel, writer, categoryName);
    }

    public void Dispose()
    {
        writer.Flush();
        writer.Dispose();
    }

    private class Logger : ILogger
    {
        private readonly ConsoleLoggerProvider provider;
        private readonly LogLevel logLevel;
        private readonly TextWriter writer;
        private readonly string categoryName;

        public Logger(ConsoleLoggerProvider provider, LogLevel logLevel, TextWriter writer, string categoryName)
        {
            this.provider = provider;
            this.logLevel = logLevel;
            this.writer = writer;
            this.categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;


        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this.logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (provider)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var oldBGColor = Console.BackgroundColor;
                var oldFGColor = Console.ForegroundColor;

                var (name, color) = GetLog(logLevel);

                try
                {
                    Console.BackgroundColor = color.BG;
                    Console.ForegroundColor = color.FG;

                    writer.Write('[');
                    writer.Write(name);

                    writer.Write(']');

                    writer.Write($"[{categoryName}]");
                    writer.Write($" {formatter(state, exception)}");
                    writer.WriteLine();
                }
                finally
                {
                    Console.BackgroundColor = oldBGColor;
                    Console.ForegroundColor = oldFGColor;
                }
            }
        }

        private static (string name, ConsoleColoring color) GetLog(LogLevel level)
        {
            return level switch
            {
                LogLevel.Critical => ("Critical", new ConsoleColoring(ConsoleColor.DarkRed, ConsoleColor.White)),
                LogLevel.Trace => ("Trace", new ConsoleColoring(ConsoleColor.White, ConsoleColor.DarkBlue)),
                LogLevel.Debug => ("Debug", new ConsoleColoring(ConsoleColor.Black, ConsoleColor.White)),
                LogLevel.Information => ("Info", new ConsoleColoring(ConsoleColor.Black, ConsoleColor.Blue)),
                LogLevel.Warning => ("Warn", new ConsoleColoring(ConsoleColor.Black, ConsoleColor.Yellow)),
                LogLevel.Error => ("Error", new ConsoleColoring(ConsoleColor.Black, ConsoleColor.Red)),
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }

        private struct ConsoleColoring(ConsoleColor bg, ConsoleColor fg)
        {
            public ConsoleColor BG = bg;
            public ConsoleColor FG = fg;
        }
    }
}
