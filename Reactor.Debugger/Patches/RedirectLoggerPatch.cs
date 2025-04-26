using System.Globalization;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Reactor.Debugger.Patches;

[HarmonyPatch(typeof(Logger))]
internal static class RedirectLoggerPatch
{
    private static readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("Among Us");

    private static bool Enabled => DebuggerConfig.RedirectLogger.Value;

    private static void Log(Logger logger, LogLevel level, Il2CppStringArray? path, Il2CppSystem.Object message, UnityEngine.Object? context = null)
    {
        var finalMessage = new StringBuilder();

        if (logger.category != Logger.Category.None) finalMessage.Append(CultureInfo.InvariantCulture, $"[{logger.category}] ");
        if (logger.subCategories != null)
        {
            foreach (var subCategory in logger.subCategories)
            {
                finalMessage.Append(CultureInfo.InvariantCulture, $"[{subCategory}] ");
            }
        }

        if (path != null)
        {
            foreach (var p in path)
            {
                finalMessage.Append(CultureInfo.InvariantCulture, $"[{p}] ");
            }
        }

        if (context != null) finalMessage.Append(CultureInfo.InvariantCulture, $"[{context.name} ({context.GetIl2CppType().FullName})] ");
        finalMessage.Append(message.ToString());

        _log.Log(level, finalMessage);
    }

    [HarmonyPatch(nameof(Logger.Debug), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool DebugPatch(Logger __instance, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Debug, null, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Info), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool InfoPatch(Logger __instance, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Info, null, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Warning), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool WarningPatch(Logger __instance, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Warning, null, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Error), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool ErrorPatch(Logger __instance, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Error, null, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Debug), typeof(Il2CppStringArray), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool DebugPatch(Logger __instance, Il2CppStringArray? path, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Debug, path, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Info), typeof(Il2CppStringArray), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool InfoPatch(Logger __instance, Il2CppStringArray? path, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Info, path, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Warning), typeof(Il2CppStringArray), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool WarningPatch(Logger __instance, Il2CppStringArray? path, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Warning, path, message, context);
        return false;
    }

    [HarmonyPatch(nameof(Logger.Error), typeof(Il2CppStringArray), typeof(Il2CppSystem.Object), typeof(UnityEngine.Object))]
    [HarmonyPrefix]
    public static bool ErrorPatch(Logger __instance, Il2CppStringArray? path, Il2CppSystem.Object message, UnityEngine.Object? context)
    {
        if (!Enabled) return true;
        Log(__instance, LogLevel.Error, path, message, context);
        return false;
    }
}
