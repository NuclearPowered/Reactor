using HarmonyLib;

namespace Reactor.Debugger.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
internal static class DisableGameEndPatch
{
    private static bool Enabled => DebuggerConfig.DisableGameEnd.Value;

    [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.CheckEndCriteria))]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlow.CheckEndCriteria))]
    [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlow.CheckEndCriteria))]
    public static bool Prefix()
    {
        return !Enabled;
    }
}
