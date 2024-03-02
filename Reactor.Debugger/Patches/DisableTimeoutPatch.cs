using HarmonyLib;
using Hazel.Udp;

namespace Reactor.Debugger.Patches;

[HarmonyPatch]
internal static class DisableTimeoutPatch
{
    private static bool Enabled => DebuggerConfig.DisableTimeout.Value;

    [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.HandleKeepAlive))]
    [HarmonyPrefix]
    public static bool DisableKeepAlive()
    {
        return !Enabled;
    }

    [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.ConnectAsync))]
    [HarmonyPostfix]
    public static void DisableTimeout(UnityUdpClientConnection __instance)
    {
        if (!Enabled) return;

        __instance.DisconnectTimeoutMs = int.MaxValue;
        __instance.ResendLimit = 0;
    }
}
