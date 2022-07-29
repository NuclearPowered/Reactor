using System.Reflection;
using HarmonyLib;

namespace Reactor.Patches
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    internal static class SplashSkip
    {
        private static void Prefix(SplashManager __instance)
        {
            __instance.minimumSecondsBeforeSceneChange = 0;
        }
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    internal static class WaitForEpicAuth
    {
        private static readonly PropertyInfo? _localUserIdProperty = typeof(EpicManager).GetProperty("localUserId", BindingFlags.Static | BindingFlags.Public);
        private static bool _loginFinished;

        private static bool Prefix(SplashManager __instance)
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
        private static class LoadPlayerPrefsPatch
        {
            private static void Postfix(bool overrideLoad)
            {
                if (overrideLoad) _loginFinished = true;
            }
        }
    }
}
