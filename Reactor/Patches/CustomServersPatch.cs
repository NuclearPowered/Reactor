using HarmonyLib;

namespace Reactor.Patches
{
    [HarmonyPatch]
    internal static class CustomServersPatch
    {
        [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
        [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__6), nameof(AuthManager._CoWaitForNonce_d__6.MoveNext))]
        [HarmonyPrefix]
        public static bool DisableAuthServerPatch(out bool __result)
        {
            __result = false;
            return false;
        }
    }
}
