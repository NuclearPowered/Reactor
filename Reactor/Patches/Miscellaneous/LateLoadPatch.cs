using HarmonyLib;
using Reactor.Utilities;

namespace Reactor.Patches.Miscellaneous;

[HarmonyPatch]
internal static class LateLoadPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPostfix]
    public static void Awake_Postfix()
    {
        PluginSingleton<ReactorPlugin>.Instance.LateLoad();
    }
}
