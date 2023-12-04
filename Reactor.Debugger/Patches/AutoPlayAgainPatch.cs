using HarmonyLib;

namespace Reactor.Debugger.Patches;

[HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowDefaultNavigation))]
internal static class AutoPlayAgainPatch
{
    public static void Postfix(EndGameNavigation __instance)
    {
        if (!DebuggerConfig.AutoPlayAgain.Value) return;

        __instance.NextGame();
    }
}
