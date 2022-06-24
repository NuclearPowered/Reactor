using System.Reflection;
using HarmonyLib;
using System;
using MonoMod.Utils;

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
        private static readonly Func<object>? _localUserId = _localUserIdProperty == null ? null : _localUserIdProperty.GetMethod.CreateDelegate<Func<object>>();

        private static bool Prefix(SplashManager __instance)
        {
            if (__instance.startedSceneLoad) return true;

            if (_localUserId != null && _localUserId() == null)
            {
                return false;
            }

            return true;
        }
    }
}
