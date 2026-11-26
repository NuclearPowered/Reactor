using HarmonyLib;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class SplashSkipPatch
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    [HarmonyPrefix]
    private static void RemoveMinimumWait(SplashManager __instance)
    {
        __instance.minimumSecondsBeforeSceneChange = 0;
    }
}
