#nullable enable
using Jint;
using Microsoft.Extensions.Logging;

namespace FortRise;

internal sealed class JSMod : Mod
{
    private Jint.Engine engine;

    public JSMod(IModContent content, IModuleContext context, ILogger logger, Jint.Engine engine, string jsPath) : base(content, context, logger)
    {
        this.engine = engine;
        string moduleName = "fort:" + Meta.Name;
        string moduleNameLib = "int:" + Meta.Name;

        engine.Modules.Add(moduleNameLib, builder =>
        {
            builder.ExportObject("content", content);
            builder.ExportObject("context", context);
            builder.ExportObject("harmony", new LuaHarmony(context.Harmony));
            builder.ExportObject("logger", new LuaLoggerWrapper(logger));
        });

        var source = content.Root.GetRelativePath(jsPath).Text;
        engine.Modules.Add(moduleName, source);

        var module = engine.Modules.Import(moduleName);
    }
}

internal sealed class LuaLoggerWrapper(ILogger logger)
{
    private ILogger logger = logger;

    public void Log(LogLevel logLevel, string message, params object[] args) => logger.Log(logLevel, message, args);
    public void LogInformation(string message, params object[] args) => logger.LogInformation(message, args);
    public void LogError(string message, params object[] args) => logger.LogError(message, args);
    public void LogCritical(string message, params object[] args) => logger.LogCritical(message, args);
    public void LogTrace(string message, params object[] args) => logger.LogTrace(message, args);
    public void LogWarning(string message, params object[] args) => logger.LogWarning(message, args);
    public void LogDebug(string message, params object[] args) => logger.LogDebug(message, args);
}