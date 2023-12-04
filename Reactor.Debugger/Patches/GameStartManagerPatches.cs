using HarmonyLib;

namespace Reactor.Debugger.Patches;

[HarmonyPatch(typeof(GameStartManager))]
internal static class GameStartManagerPatches
{
    [HarmonyPatch(nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void StartPatch(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }

    [HarmonyPatch(nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    public static void UpdatePatch(GameStartManager __instance)
    {
        __instance.countDownTimer = 0;
    }
}
