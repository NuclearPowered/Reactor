using System;
using HarmonyLib;
using Reactor.Utilities;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal static class PingTrackerPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PingTracker __instance)
    {
        if (!__instance.text.text.EndsWith("\n", StringComparison.InvariantCulture)) __instance.text.text += "\n";
        __instance.text.text += ReactorPingTracker.GetPingTrackerText();
    }
}
