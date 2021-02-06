using HarmonyLib;

namespace Reactor.Debugger
{
    internal static class Patches
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class UpdatePatch
        {
            public static void Prefix(GameStartManager __instance)
            {
                __instance.MinPlayers = 1;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class CountdownPatch
        {
            public static void Prefix(GameStartManager __instance)
            {
                __instance.countDownTimer = 0;
            }
        }

        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
        [HarmonyPriority(Priority.First)]
        public static class CheckEndCriteriaPatch
        {
            public static bool Prefix()
            {
                return !PluginSingleton<DebuggerPlugin>.Instance.Component.DisableGameEnd;
            }
        }
    }
}
