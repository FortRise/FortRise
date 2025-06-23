using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FortLauncher;

internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly LogLevel logLevel;
    private readonly string logFileName;
    private readonly TextWriter writer;

    public FileLoggerProvider(string directoryPath, LogLevel logLevel)
    {
        this.logLevel = logLevel;

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var files = Directory.GetFiles(directoryPath).Where(x => Path.GetExtension(x) == ".txt").ToArray();
        if (files.Length > 10)
        {
            var file = files[0];
            File.Delete(file);
        }

        var dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        logFileName = Path.Combine(directoryPath, $"{dateTime}.txt");

        writer = new StreamWriter(logFileName, Encoding.UTF8, new()
        {
            Access = FileAccess.Write,
            BufferSize = 1,
            Mode = FileMode.CreateNew,
            Options = FileOptions.Asynchronous
        })
        {
            AutoFlush = true
        };

    }

    public ILogger CreateLogger(string categoryName)
    {
        return new Logger(this, writer, logLevel, categoryName);
    }

    public void Dispose()
    {
        writer.Flush();
        writer.Dispose();
    }

    private class Logger : ILogger
    {
        private readonly LogLevel logLevel;
        private readonly string categoryName;
        private readonly FileLoggerProvider provider;
        private readonly TextWriter writer;

        public Logger(FileLoggerProvider provider, TextWriter writer, LogLevel logLevel, string categoryName)
        {
            this.logLevel = logLevel;
            this.categoryName = categoryName;
            this.provider = provider;
            this.writer = writer;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

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

                var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                writer.WriteLine($"[{dateTime}][{logLevel}][{categoryName}] {formatter(state, exception)}");
            }
        }
    }
}
