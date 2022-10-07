using System.Runtime.CompilerServices;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace Reactor.Utilities;

public static class Logger<T> where T : BasePlugin
{
    public static ManualLogSource Instance => PluginSingleton<T>.Instance.Log;

    /// <inheritdoc cref="ManualLogSource.Log(BepInEx.Logging.LogLevel,object)"/>
    public static void Log(LogLevel level, object data) => Instance.Log(level, data);

    /// <inheritdoc cref="ManualLogSource.LogFatal(object)"/>
    public static void Log(LogLevel level, [InterpolatedStringHandlerArgument("level")] BepInExLogInterpolatedStringHandler logHandler) => Instance.Log(level, logHandler);

    /// <inheritdoc cref="ManualLogSource.LogFatal(object)"/>
    public static void Fatal(object data) => Instance.LogFatal(data);

    /// <inheritdoc cref="ManualLogSource.LogFatal(BepInExFatalLogInterpolatedStringHandler)"/>
    public static void Fatal(BepInExFatalLogInterpolatedStringHandler logHandler) => Instance.LogFatal(logHandler);

    /// <inheritdoc cref="ManualLogSource.LogError(object)"/>
    public static void Error(object data) => Instance.LogError(data);

    /// <inheritdoc cref="ManualLogSource.LogError(BepInExErrorLogInterpolatedStringHandler)"/>
    public static void Error(BepInExErrorLogInterpolatedStringHandler logHandler) => Instance.LogError(logHandler);

    /// <inheritdoc cref="ManualLogSource.LogWarning(object)"/>
    public static void Warning(object data) => Instance.LogWarning(data);

    /// <inheritdoc cref="ManualLogSource.LogWarning(BepInExWarningLogInterpolatedStringHandler)"/>
    public static void Warning(BepInExWarningLogInterpolatedStringHandler logHandler) => Instance.LogWarning(logHandler);

    /// <inheritdoc cref="ManualLogSource.LogMessage(object)"/>
    public static void Message(object data) => Instance.LogMessage(data);

    /// <inheritdoc cref="ManualLogSource.LogMessage(BepInExMessageLogInterpolatedStringHandler)"/>
    public static void Message(BepInExMessageLogInterpolatedStringHandler logHandler) => Instance.LogMessage(logHandler);

    /// <inheritdoc cref="ManualLogSource.LogInfo(object)"/>
    public static void Info(object data) => Instance.LogInfo(data);

    /// <inheritdoc cref="ManualLogSource.LogInfo(BepInExInfoLogInterpolatedStringHandler)"/>
    public static void Info(BepInExInfoLogInterpolatedStringHandler logHandler) => Instance.LogInfo(logHandler);

    /// <inheritdoc cref="ManualLogSource.LogDebug(object)"/>
    public static void Debug(object data) => Instance.LogDebug(data);

    /// <inheritdoc cref="ManualLogSource.LogDebug(BepInExDebugLogInterpolatedStringHandler)"/>
    public static void Debug(BepInExDebugLogInterpolatedStringHandler logHandler) => Instance.LogDebug(logHandler);
}
