using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities;
using UnityEngine;

namespace Reactor.Debugger;

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

    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    public static class CheckEndCriteriaPatch
    {
        [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.CheckEndCriteria))]
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlow.CheckEndCriteria))]
        [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlow.CheckEndCriteria))]
        public static bool Prefix()
        {
            return !PluginSingleton<DebuggerPlugin>.Instance.Component.DisableGameEnd;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    public static class GameSettingMenuPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            // Unlocks map/impostor amount changing in online (for testing on your custom servers)
            __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    [HarmonyPriority(Priority.First)]
    public static class GameOptionsMenuPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;

            __instance.Children
                .Single(o => o.Title == StringNames.GameNumImpostors)
                .Cast<NumberOption>()
                .ValidRange = new FloatRange(0, byte.MaxValue);
        }
    }
}
