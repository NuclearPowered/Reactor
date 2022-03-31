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
}
