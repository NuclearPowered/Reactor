using System;
using System.Globalization;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace Reactor.Debugger.Patches;

[HarmonyPatch(typeof(Logger))]
internal static class RedirectLoggerPatch
{
    private static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Among Us");

    private static bool Enabled => DebuggerConfig.RedirectLogger.Value;

    private static void Log(Logger logger, Logger.Level level, Il2CppSystem.Object message, Object? context = null)
    {
        var finalMessage = new StringBuilder();

        if (logger.category != Logger.Category.None) finalMessage.Append(CultureInfo.InvariantCulture, $"[{logger.category}] ");
        if (!string.IsNullOrEmpty(logger.tag)) finalMessage.Append(CultureInfo.InvariantCulture, $"[{logger.tag}] ");
        if (context != null) finalMessage.Append(CultureInfo.InvariantCulture, $"[{context.name} ({context.GetIl2CppType().FullName})]");
        finalMessage.Append(CultureInfo.InvariantCulture, $" {message.ToString()} ");

        _log.Log(
            level switch
            {
                Logger.Level.Debug => LogLevel.Debug,
                Logger.Level.Error => LogLevel.Error,
                Logger.Level.Warning => LogLevel.Warning,
                Logger.Level.Info => LogLevel.Info,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
            },
            finalMessage
        );
    }

    [HarmonyPatch(nameof(Logger.Debug))]
    [HarmonyPrefix]
    public static bool DebugPatch(Logger __instance, Il2CppSystem.Object message, Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, Logger.Level.Debug, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Info))]
    [HarmonyPrefix]
    public static bool InfoPatch(Logger __instance, Il2CppSystem.Object message, Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, Logger.Level.Info, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Warning))]
    [HarmonyPrefix]
    public static bool WarningPatch(Logger __instance, Il2CppSystem.Object message, Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, Logger.Level.Warning, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Error))]
    [HarmonyPrefix]
    public static bool ErrorPatch(Logger __instance, Il2CppSystem.Object message, Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, Logger.Level.Error, message, context);
        return false;
    }
}
