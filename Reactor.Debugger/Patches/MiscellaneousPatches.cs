using HarmonyLib;

namespace Reactor.Debugger.Patches;

[HarmonyPatch]
internal static class MiscellaneousPatches
{
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    [HarmonyPrefix]
    public static void AmBannedPatch(out bool __result)
    {
        __result = false;
    }
}
