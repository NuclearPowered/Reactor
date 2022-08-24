using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;

namespace Reactor;

public static class Logger<T> where T : BasePlugin
{
    public static ManualLogSource Instance => PluginSingleton<T>.Instance.Log;

    /// <inheritdoc cref="ManualLogSource.LogFatal"/>
    public static void Fatal(object data) => Instance.LogFatal(data);

    /// <inheritdoc cref="ManualLogSource.LogError"/>
    public static void Error(object data) => Instance.LogError(data);

    /// <inheritdoc cref="ManualLogSource.LogWarning"/>
    public static void Warning(object data) => Instance.LogWarning(data);

    /// <inheritdoc cref="ManualLogSource.LogMessage"/>
    public static void Message(object data) => Instance.LogMessage(data);

    /// <inheritdoc cref="ManualLogSource.LogInfo"/>
    public static void Info(object data) => Instance.LogInfo(data);

    /// <inheritdoc cref="ManualLogSource.LogDebug"/>
    public static void Debug(object data) => Instance.LogDebug(data);
}
