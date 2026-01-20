using System;
using HarmonyLib;
using Reactor.Utilities;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
internal static class PingTrackerPatch
{
    private static string? _extraText;
    private static string? ExtraText => _extraText ??= ReactorCredits.GetText(ReactorCredits.Location.PingTracker);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PingTracker __instance)
    {
        if (ExtraText == null)
        {
            return;
        }

        if (!__instance.text.text.EndsWith("\n", StringComparison.InvariantCulture))
        {
            __instance.text.text += "\n";
        }
        __instance.text.text += ExtraText;
    }
}
