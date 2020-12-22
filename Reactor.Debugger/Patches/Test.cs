using System.Linq;
using HarmonyLib;

namespace Reactor.Debugger.Patches
{
    public static class Test
    {
        static Test()
        {
            GameOptionsData.MaxImpostors = GameOptionsData.RecommendedImpostors = Enumerable.Repeat((int) byte.MaxValue, byte.MaxValue).ToArray();
            GameOptionsData.MinPlayers = Enumerable.Repeat(1, 4).ToArray();
        }

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
            public static bool Prefix(ref bool __result)
            {
                return __result = false;
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
