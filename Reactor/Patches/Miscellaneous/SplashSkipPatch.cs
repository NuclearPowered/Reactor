using System.Reflection;
using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class SplashSkipPatch
{
    private static readonly PropertyInfo? _localUserIdProperty = typeof(EpicManager).GetProperty("localUserId", BindingFlags.Static | BindingFlags.Public);
    private static bool _loginFinished;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    [HarmonyPrefix]
    private static void RemoveMinimumWait(SplashManager __instance)
    {
        __instance.minimumSecondsBeforeSceneChange = 0;
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    [HarmonyPrefix]
    private static bool WaitForLogin(SplashManager __instance)
    {
        if (__instance.startedSceneLoad) return true;

        if (_localUserIdProperty != null && !_loginFinished)
        {
            return false;
        }

        return true;
    }

    // EpicManager calls SaveManager.LoadPlayerPrefs(true) both on successful and unsuccessful EOS login
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadPlayerPrefs))]
    [HarmonyPostfix]
    private static void WaitForEpicAuth(bool overrideLoad)
    {
        if (overrideLoad) _loginFinished = true;
    }
}
